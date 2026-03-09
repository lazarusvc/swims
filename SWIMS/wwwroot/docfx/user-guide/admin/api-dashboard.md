# API Dashboard

*Requires `Admin.ApiDashboard` permission.*

Navigate to **Admin → API Dashboard**.

The API Dashboard provides a live inventory of all REST API endpoints registered in SWIMS, along with their authentication requirements and real-time health probes.

## What the Dashboard Shows

### Summary Cards

- **Total Endpoints** — total number of registered `/api/v1/*` routes
- **Authenticated** — endpoints that require a logged-in user
- **Anonymous** — endpoints that are publicly accessible (health checks, etc.)

### Endpoint Table

Each row in the table shows:

| Column | Description |
|--------|-------------|
| **Route Pattern** | The URL pattern (e.g. `/api/v1/beneficiary/{id}`) |
| **Methods** | HTTP methods supported (GET, POST, PUT, DELETE) |
| **Tag** | Module grouping (e.g. Data, Messaging, Notifications) |
| **Auth** | Whether authentication is required |
| **Status** | Live HTTP status probe result (shown for GET/HEAD endpoints with no route parameters) |

### Filters

Filter the endpoint table by:

- **Method** (GET, POST, PUT, DELETE)
- **Tag** (module)
- **Auth status** (authenticated / anonymous)

## Status Probes

For simple GET endpoints (no required route parameters), the dashboard automatically sends a HEAD request and shows the HTTP response code:

| Code | Meaning |
|------|---------|
| `200 OK` | Endpoint is healthy and responding |
| `401 Unauthorized` | Endpoint requires auth (as expected — probe is unauthenticated) |
| `403 Forbidden` | Auth passed but permission denied |
| `404 Not Found` | Endpoint route not resolving correctly |
| `500` | Server error — investigate immediately |

> [!TIP]
> Use the status probe results to quickly spot any endpoints returning unexpected errors after a deployment.
