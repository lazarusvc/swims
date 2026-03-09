# Two-Factor Authentication

SWIMS supports **TOTP-based two-factor authentication** (Time-based One-Time Passwords) using ASP.NET Core Identity's built-in authenticator support, compatible with any standard authenticator app (Google Authenticator, Microsoft Authenticator, Authy, etc.).

## How It Works

1. User navigates to **Account → Two-Factor Authentication**.
2. SWIMS generates a TOTP secret and displays a **QR code** (rendered via `QRCoder`) and a manual setup key.
3. User scans the QR code in their authenticator app and enters the 6-digit verification code.
4. SWIMS verifies the code and **enables 2FA** on the account, displaying recovery codes.
5. On subsequent logins, after the password check succeeds, the user is redirected to `LoginWith2fa` and must enter their current TOTP code.

## Recovery Codes

- SWIMS generates **10 recovery codes** when 2FA is enabled.
- Each recovery code is single-use.
- Users can regenerate recovery codes at any time from the Account Manage pages.
- Lost recovery codes + no access to the authenticator app = account locked out (admin reset required).

## Manage Pages

| Page | Purpose |
|------|---------|
| `TwoFactorAuthentication.cshtml` | 2FA status overview; enable/disable |
| `EnableAuthenticator.cshtml` | QR code + manual key + verification |
| `ResetAuthenticator.cshtml` | Invalidates current authenticator, forces re-setup |
| `GenerateRecoveryCodes.cshtml` | Regenerate recovery codes |
| `Disable2fa.cshtml` | Confirm and disable 2FA |
| `ShowRecoveryCodes.cshtml` | Display newly generated recovery codes (one-time) |
| `LoginWith2fa.cshtml` | TOTP entry during login |
| `LoginWithRecoveryCode.cshtml` | Recovery code login fallback |

## LDAP Users

LDAP (`IsLdapUser=true`) accounts **cannot enable 2FA** from within SWIMS. Their security is managed entirely through Active Directory (smart card, Entra MFA, etc.). The 2FA menu is hidden for these accounts.

## Token Provider

The `DefaultAuthenticatorProvider` is used:

```csharp
options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
```

TOTP window is the ASP.NET Identity default (±1 time step = ±30 seconds tolerance).

## Admin Recovery

If a user is locked out of their 2FA, an administrator can:

1. Navigate to **Admin → Users**.
2. Open the user's profile.
3. Reset their 2FA via the admin controls, which calls `UserManager.ResetAuthenticatorKeyAsync` + `UserManager.SetTwoFactorEnabledAsync(false)`.
