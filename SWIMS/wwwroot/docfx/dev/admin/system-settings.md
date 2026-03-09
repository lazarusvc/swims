# System Settings

**Route**: `GET /Admin/SystemSettings`  
**Permission**: `Admin.Settings`

System Settings provides runtime-configurable application parameters that can be changed by administrators without redeploying or editing configuration files.

## Architecture

```
SystemSettingsController
  └── ISystemSettingsService / SystemSettingsService
        └── reads/writes SystemSettings entity (SwimsIdentityDbContext)
        └── caches values in IMemoryCache
```

Settings are stored in the database and cached in-process. Cache is invalidated on save.

## `SystemSettingsModels`

```csharp
public class SystemSettingsModel
{
    public int Id { get; set; }
    
    // Identity / Branding
    public string? SiteName { get; set; }           // e.g., "SWIMS"
    public string? SiteTagline { get; set; }
    public string? SiteLogoUrl { get; set; }
    
    // Contact
    public string? SupportEmail { get; set; }       // Used in email templates
    public string? SupportPhone { get; set; }        // Used in email templates
    public string? OrganisationName { get; set; }   // e.g., "Ministry of Social Services"
    
    // Features
    public bool EnableRegistration { get; set; }     // Allow public user self-registration
    public bool MaintenanceMode { get; set; }        // Show maintenance banner
    public string? MaintenanceMessage { get; set; }
    
    // Email
    public string? NotificationFromName { get; set; } // "From" name for system emails
    public string? NotificationFromEmail { get; set; }
}
```

## Editing Settings

1. Navigate to **Admin → System Settings**.
2. All current settings are shown in a form.
3. Edit any value and click **Save**.
4. The in-memory cache is cleared; the new values take effect on the next request.

## Using Settings in Views and Services

Inject `ISystemSettingsService`:

```csharp
public class SomeController : Controller
{
    private readonly ISystemSettingsService _settings;
    
    public SomeController(ISystemSettingsService settings)
    {
        _settings = settings;
    }
    
    public async Task<IActionResult> Index()
    {
        var settings = await _settings.GetAsync();
        ViewBag.SiteName = settings.SiteName;
        // ...
    }
}
```

In Razor views, `ISystemSettingsService` can be injected directly via `@inject`.

## Site Identity

The Site Identity section of System Settings feeds the header branding in the WowDash layout: site name, logo, and tagline. Changes here take effect immediately for all users on next page load.

## Maintenance Mode

When `MaintenanceMode = true`, a banner is shown at the top of all pages (or the app can be configured to block all non-admin users from accessing protected routes).
