# Background Jobs — Overview

SWIMS uses **Hangfire** (v1.8, SQL Server storage) for background job processing. Jobs are stored in the `ops` schema and managed via the Hangfire Dashboard.

## Hangfire Configuration

```csharp
builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseConsole()
       .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
       {
           SchemaName = "ops",
           PrepareSchemaIfNecessary = true,
           SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
           QueuePollInterval = TimeSpan.FromSeconds(1),
           CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
           UseRecommendedIsolationLevel = true
       });
});
```

## Queue Architecture

Three Hangfire servers, each processing a dedicated queue:

| Server | Queue | Workers | Processes |
|--------|-------|---------|----------|
| Notifications | `notifications` | 10 | Elsa workflow dispatch, notification delivery jobs |
| Email Outbox | `outbox` | 1 | Email outbox dispatch |
| Default | `default` | 1 | Miscellaneous unqueued jobs |

The outbox server is intentionally restricted to 1 worker to prevent email blast scenarios.

## Recurring Jobs

Registered on startup when `Hangfire:ScheduleOnStartup = true`:

| Job ID | Schedule | Class | Method |
|--------|---------|-------|--------|
| `email-outbox-dispatch` | Every minute | `EmailOutboxJobs` | `RunOnceAsync(50)` |
| `notification-digest-daily` | Daily at 08:00 | `NotificationDigestJobs` | `RunDailyAsync()` |

## Job Inventory

### `EmailOutboxJobs.RunOnceAsync(batchSize)`

Dequeues up to `batchSize` rows from `notify.email_outbox` where `SentAt IS NULL` and `RetryCount < MaxRetries`, and sends them via `IEmailService`.

- On success: stamps `SentAt`.
- On failure: increments `RetryCount`; logs the error. After `MaxRetries`, marks as permanently failed.

### `NotificationDigestJobs.RunDailyAsync()`

For each user who has `digest = true` for any notification type:

1. Aggregates unsent digest-eligible notifications from the past 24 hours.
2. Groups by type.
3. Composes a single summary email via `NotificationEmailComposer`.
4. Enqueues to `IEmailOutbox`.
5. Marks processed notifications as digest-sent.

### `NotificationDispatchJobs` / `NotificationDeliveryJobs`

Handle the Hangfire-queued phase of notification delivery (web push + email) when `INotificationDispatcher` is used instead of inline `INotifier`.

### `ElsaWorkflowJobs`

Processes the Elsa workflow queue (`IElsaWorkflowQueue`) — dequeues workflow trigger payloads and calls `IElsaWorkflowClient.ExecuteByNameAsync`. Failed Elsa calls are logged and do not retry by default (fire-and-forget with graceful degradation).

## Hosted Services (Non-Hangfire Background Work)

Two `IHostedService` implementations run directly on the ASP.NET Core host (not Hangfire):

| Service | Purpose |
|---------|---------|
| `CaseBenefitExpiryHostedService` | Marks Active cases as Inactive when benefit_end_at has passed |
| `CaseBeneficiaryPeriodSweepHostedService` | Expires timed overrides + applies benefit expiry rule |

These run on a timer (configurable poll interval) and are appropriate for the low-frequency, non-distributed sweep tasks.

## Hangfire Dashboard

**Route**: `/hangfire`  
**Permission**: `Admin.Hangfire`

The standard Hangfire dashboard is secured by requiring the `Admin.Hangfire` permission claim. It provides:

- Recurring job list and status
- Enqueued / processing / failed / succeeded job queues
- Job retry controls
- Server health

> [!WARNING]
> The Hangfire dashboard exposes job arguments and return values. In production, ensure only trusted administrators have the `Admin.Hangfire` permission.
