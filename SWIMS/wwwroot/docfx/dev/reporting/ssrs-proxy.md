# SSRS Proxy

The SSRS Proxy (`SsrsProxyController`) is an HTTP reverse proxy that forwards requests to the SSRS report server using the application's Windows identity. This allows browsers to view SSRS reports without requiring each user to have direct SSRS network access or Windows credentials.

## How It Works

1. The report viewer page (`Reports/View`) loads a report in an `<iframe>` with `src="/ssrs-proxy?url=<base64-encoded-ssrs-url>"`.
2. `SsrsProxyController` decodes the URL, validates it is within the allowed SSRS base URL, and forwards the request using the `ssrs-proxy` named `HttpClient`.
3. The HTTP client uses `UseDefaultCredentials = true`, so the application pool identity authenticates to SSRS via Kerberos/NTLM.
4. The SSRS response (HTML4.0 rendered report) is streamed back to the browser.

## Security Constraints

- The proxy validates that the forwarded URL starts with the configured `Reporting:SsrsBaseUrl`. This prevents the proxy from being used to forward requests to arbitrary URLs.
- Only authenticated SWIMS users with `Reports.View` permission can load the report viewer page and thus trigger proxy requests.
- Direct access to `/ssrs-proxy` without the outer reports authorization check is still subject to the global fallback authentication policy.

## `HttpClient` Configuration

```csharp
builder.Services.AddHttpClient("ssrs-proxy", c =>
{
    c.Timeout = TimeSpan.FromSeconds(180); // tolerate slow first renders
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseDefaultCredentials = true,
    PreAuthenticate = true,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.GZip | 
                             DecompressionMethods.Deflate | 
                             DecompressionMethods.Brotli,
    UseCookies = true,
    CookieContainer = new CookieContainer(),
    UseProxy = false
});
```

- `PreAuthenticate = true` skips the 401 challenge round-trip on subsequent requests to the same SSRS endpoint.
- `AllowAutoRedirect = false` passes redirects back to the browser rather than following them server-side.
- `UseCookies = true` with a dedicated `CookieContainer` per handler maintains session cookies to the SSRS server across requests.

## Deployment Requirements

For Windows authentication to work:

1. **IIS Application Pool**: run under an account with SSRS Browse/View permission.
2. **Kerberos SPN**: if the SSRS server is accessed by hostname (not IP), ensure the SPN is registered: `HTTP/reportserver` for the service account.
3. **Network access**: the SWIMS server must be able to reach the SSRS server on port 80/443.
4. **SSRS permissions**: the app pool account must have at minimum **Browser** role on the target SSRS folder.

## Timeouts

The 180-second timeout is set to tolerate complex reports or large datasets on first render. For reports that consistently take longer, adjust this value in configuration or per-report via a `Timeout` property on the `SwReport` model (future enhancement).
