# Elsa Workflows — Overview

SWIMS integrates with an **Elsa v3** workflow engine running as an external service. Elsa handles complex multi-step notification and automation workflows that would be difficult to express as simple in-process code.

## Architecture

```
SWIMS (this app)
  │
  ├── IElsaWorkflowClient ──HTTP──► Elsa v3 Server
  │       (ExecuteByNameAsync)         /api/workflow-definitions/{id}/execute
  │
  └── IElsaWorkflowQueue ──Hangfire──► ElsaWorkflowJobs
          (EnqueueByNameAsync)              └── IElsaWorkflowClient.ExecuteByNameAsync
```

Elsa calls from SWIMS are **non-blocking**. If Elsa is unavailable, the app continues normally — a warning is shown once per request and the operation is logged. This ensures availability is not coupled to the Elsa server.

## Configuration

```json
"Elsa": {
  "ServerUrl": "http://localhost:5001/",
  "ApiKey": "your-elsa-api-key",
  "Integration": {
    "NotificationsKey": "shared-secret-for-elsa-callbacks"
  }
}
```

- `ServerUrl`: Base URL of the Elsa v3 instance (ensure trailing slash).
- `ApiKey`: Bearer token (`Authorization: ApiKey <key>`) for SWIMS → Elsa requests.
- `Integration:NotificationsKey`: Shared secret used by Elsa when calling back into SWIMS via the `NotificationsIntegrationController`.

> [!IMPORTANT]
> `Elsa:Integration:NotificationsKey` is validated on startup — an empty value will prevent the application from starting.

## `IElsaWorkflowClient`

```csharp
public interface IElsaWorkflowClient
{
    Task ExecuteByNameAsync(
        string workflowName,
        object? input = null,
        CancellationToken ct = default,
        bool throwOnUnavailable = false);
}
```

`ElsaWorkflowClient`:
1. Queries Elsa's workflow definition API to resolve `workflowName` → `definitionId` (prefers Published, falls back to Latest).
2. POSTs to `/workflow-definitions/{id}/execute` with the input payload.
3. On Elsa unavailability (connection refused, timeout, 4xx/5xx): logs a warning, optionally shows a TempData warning to the user, and returns gracefully.

## `IElsaWorkflowQueue`

For non-blocking fire-and-forget triggers from web request handlers:

```csharp
public interface IElsaWorkflowQueue
{
    Task EnqueueByNameAsync(string workflowName, object? input = null);
}
```

`ElsaWorkflowQueue` enqueues a Hangfire job (`ElsaWorkflowJobs`) on the `notifications` queue. This decouples the web request from the Elsa HTTP call — the request completes immediately and Elsa execution happens in the background.

## Registered Workflow Names

| Workflow Name | Trigger | Purpose |
|---------------|---------|---------|
| `Swims.Notifications.DirectInApp` | Approval actions, case events | Triggers Elsa to dispatch in-app notification flow |

Additional workflow names are defined in Elsa and must match the string passed to `ExecuteByNameAsync`.

## `NotificationsIntegrationController`

**Route**: `POST /Integration/Notifications`

Elsa callbacks into SWIMS via this controller. The shared key (`Elsa:Integration:NotificationsKey`) is verified via `ElsaIntegrationKeyFilter` before the action runs.

This allows Elsa workflows to trigger SWIMS actions (e.g., create a notification, update a case status) after completing multi-step automation.

## Graceful Degradation

- Elsa unavailability **does not fail** the user's request.
- A yellow warning banner (`TempData`) is shown: *"Workflow/notifications are temporarily unavailable. Your changes were saved."*
- The warning is shown **once per request** (checked via a flag in `TempData` to prevent duplicate messages across redirects).
- All data changes are committed to the SWIMS DB before the Elsa call — Elsa failing never rolls back a save.

## Deployment

Elsa v3 runs as a separate ASP.NET Core application. For the full setup and IIS subdirectory deployment checklist, refer to the archived Notion docs: *"Elsa Publish Checklist — IIS Subdirectory"*.
