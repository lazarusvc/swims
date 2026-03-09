# API Module — Overview

SWIMS exposes a **versioned REST API** under `/api/v1`. All API endpoints are registered by `ApiEndpoints.cs` (`Web/Endpoints/ApiEndpoints.cs`) and require the `Api.Access` permission unless explicitly marked otherwise.

## Route Structure

```
/api
└── /v1
    ├── /healthz                    CoreEndpoints — health check (public)
    ├── /readyz                     CoreEndpoints — readiness check (public)
    ├── /me/heartbeat               CoreEndpoints — authenticated heartbeat
    ├── /me/chats/*                 MessagingEndpoints
    ├── /me/notifications/*         NotificationsEndpoints
    ├── /me/push/*                  PushEndpoints
    ├── /ops/logs/*                 OperationsEndpoints
    ├── /beneficiary/*              Data\BeneficiaryEndpoints
    ├── /city/*                     Data\CityEndpoints
    ├── /financial_institution/*    Data\FinancialInstitutionEndpoints
    ├── /organization/*             Data\OrganizationEndpoints
    └── /meta/endpoints             MetaEndpoints — runtime route inventory
    └── /meta/ping                  MetaEndpoints — simple ping
```

## Aggregator: `ApiEndpoints.cs`

`ApiEndpoints.cs` is the single composition root for all API modules:

```csharp
public static class ApiEndpoints
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");
        var v1  = api.MapGroup("/v1");

        v1.MapCoreEndpoints();
        v1.MapMessagingEndpoints();
        v1.MapNotificationsEndpoints();
        v1.MapPushEndpoints();
        v1.MapOperationsEndpoints();
        v1.MapDataEndpoints();   // city, beneficiary, financial_institution, organization
        v1.MapMetaEndpoints();

        return app;
    }
}
```

Each module uses **group-relative routes** (e.g., `"beneficiary"`, `"/{id}"`). The `/api/v1` prefix is applied by the aggregator.

## Data Endpoints

Data endpoints follow a consistent CRUD pattern:

```
GET    /api/v1/{resource}        List (with optional ?q= search)
GET    /api/v1/{resource}/{id}   Get by ID
POST   /api/v1/{resource}        Create — returns 201 Created
PUT    /api/v1/{resource}/{id}   Update — returns 200 OK
DELETE /api/v1/{resource}/{id}   Delete — returns 204 No Content
```

## MetaEndpoints

`MetaEndpoints` expose the runtime route inventory for the API Dashboard:

| Route | Response |
|-------|---------|
| `GET /api/v1/meta/endpoints` | JSON array of all registered routes with pattern, methods, tags, auth requirements |
| `GET /api/v1/meta/ping` | `{ "ok": true, "utc": "..." }` |

### Endpoint Metadata Shape

```json
{
  "pattern": "/api/v1/beneficiary",
  "methods": ["GET", "POST"],
  "displayName": "BeneficiaryEndpoints",
  "tags": ["Data"],
  "requiresAuth": true,
  "allowAnonymous": false,
  "isApi": true,
  "isV1": true
}
```

## API Dashboard

**Route**: `GET /Admin/Api/Dashboard`  
**Permission**: `Admin.ApiDashboard`

The API Dashboard (`Areas/Admin/Views/Api/Dashboard.cshtml`) provides:

- Summary cards: total endpoints, authenticated endpoints, anonymous endpoints
- Filterable table: filter by HTTP method, tag, and auth status
- **Status probe**: for `GET`/`HEAD`-only endpoints without route parameters, sends a live HEAD request and shows the HTTP status code
- Inline JavaScript (WowDash-compatible layout)

## PathBase Support

All API client JavaScript uses `window.apiUrl(path)` / `api(path)` helper functions that prepend the current `PathBase`. This ensures API calls work correctly under subdirectory deployments (e.g., `/swims/api/v1/...`).

## Authentication

All API endpoints require a valid SWIMS session cookie by default (the global fallback policy applies). For programmatic API access, include the session cookie in requests.

The `Api.Access` permission is required on the requesting user's account. This allows granting API access selectively — a batch processing account can have `Api.Access` without any other permissions.
