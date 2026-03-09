# Reporting — Overview

SWIMS integrates with **SQL Server Reporting Services (SSRS)** to deliver operational reports. Reports are defined in the database and rendered inline in the browser via a **reverse proxy** that forwards requests to the SSRS report server using Windows authentication.

## Architecture

```
Browser
  │
  ▼
GET /Reports/{reportId}       (requires Reports.View)
  │
  ▼
ReportsController
  └── SsrsUrlBuilder.BuildUrl(report, params)
        └── Constructs SSRS URL with rs:Command=Render&rs:Format=HTML4.0&...params
  │
  ▼
<iframe src="/ssrs-proxy?url=<encodedSsrsUrl>" />
  │
  ▼
SsrsProxyController (/ssrs-proxy)
  └── HttpClient("ssrs-proxy")
      ├── UseDefaultCredentials = true  (Windows auth to SSRS)
      └── Streams response back to browser
```

## Configuration

```json
"Reporting": {
  "SsrsBaseUrl": "http://reportserver/ReportServer/Pages/ReportViewer.aspx",
  "ReportServerPath": "/Reports/SWIMS/"
}
```

The `ssrs-proxy` HTTP client is configured in `Program.cs` with `UseDefaultCredentials = true`, meaning the application pool identity is used for Windows authentication to the SSRS server. Ensure the app pool account has at least **Browser** role on the SSRS report server.

## Report Definitions (`SwReport`)

```csharp
public class SwReport
{
    public int Id { get; set; }
    public string Name { get; set; }          // Display name
    public string ReportPath { get; set; }    // SSRS path, e.g. "/SWIMS/CaseList"
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
```

### Report Parameters (`SwReportParam`)

```csharp
public class SwReportParam
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public string ParamName { get; set; }     // SSRS parameter name
    public string Label { get; set; }         // Display label in UI
    public string InputType { get; set; }     // "text", "date", "select"
    public string? Options { get; set; }      // JSON array for select inputs
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}
```

## Routes

| Route | Permission | Purpose |
|-------|-----------|---------|
| `GET /Reports` | `Reports.View` | Reports index (list all active reports) |
| `GET /Reports/View/{id}` | `Reports.View` | View/run a report with parameter form |
| `/ssrs-proxy` | *(internal)* | Proxies requests to SSRS server |
| `GET /Admin/ReportsAdmin` | `Reports.Admin` | Manage report definitions |
| `GET /Admin/ReportParams` | `Reports.Admin` | Manage report parameters |

## PDF Export

The SSRS URL builder can generate a PDF export URL by changing `rs:Format=PDF`. The report view UI provides a **Download as PDF** button that navigates to the proxy URL with PDF format.

## Inline Viewer

Reports are displayed inline using an `<iframe>` pointing to the proxy endpoint. The proxy strips SSRS navigation chrome (the toolbar) by appending `rc:Toolbar=false` to the URL, giving a clean embedded view.

## Related Pages

- [SSRS Proxy Details](ssrs-proxy.md)
