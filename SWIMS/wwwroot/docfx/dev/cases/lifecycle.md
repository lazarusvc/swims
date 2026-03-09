# Case Lifecycle & Status

## Lifecycle Service

`ICaseLifecycleService` / `CaseLifecycleService` (`Services/Cases/`) is the single authority for case status computation. Controllers and background services call it rather than mutating status directly.

```csharp
public interface ICaseLifecycleService
{
    Task RefreshAsync(int caseId, CancellationToken ct = default);
}
```

`RefreshAsync` performs:

1. Load the case and its linked form submissions.
2. Find the **Primary Application** link (`SW_caseForm.is_primary_application = true`).
3. Load the Primary Application's `SW_formProcess` records.
4. Interpret the approval decision defensively (checks multiple possible field names for the approved/rejected signal).
5. Compute the target status using the rules below.
6. Apply any active manual override (override always wins if its expiry hasn't passed).
7. Persist the updated `status`, `benefit_start_at`, `benefit_end_at`, `benefit_period_months`, `benefit_period_source`.

## Status Computation Rules

```
Has Primary Application?
├── No → status = Pending
└── Yes → read approval decision
        ├── Approved → benefit end in past? → Inactive
        │              benefit end future/null? → Active
        ├── Rejected/Denied → Closed
        └── Unknown/Pending → Pending
```

### Manual Override

If a manual status override is set **and** has not yet expired:

- The computed status is replaced by the override value.
- A timed override (`override_status_until` is set) is honoured until that date; after which the background sweep clears it.
- An indefinite override (`override_status_until = null`) persists until manually cleared.

> [!IMPORTANT]
> The override always wins during `RefreshAsync`. Setting an override effectively freezes the status until the override expires or is manually removed.

## Primary Application Linking

Only one form submission per case can be marked `is_primary_application = true`. When linking a new primary:

1. The controller clears `is_primary_application` on any existing primary link.
2. The new link is inserted with `is_primary_application = true`.
3. `CaseLifecycleService.RefreshAsync` is called to immediately recalculate status/period.

> [!NOTE]
> **Auto-linking is intentionally disabled.** Even if a form submission is related to the case's programme, it will not be auto-linked as the Primary Application. A user must explicitly link submissions.

## Background Sweeps

Two `IHostedService` implementations run on the ASP.NET Core host:

### `CaseBenefitExpiryHostedService`

Runs on a timer (configurable interval, defaults to hourly).

For each case where:
- `status = "Active"`
- `benefit_end_at <= UtcNow`
- No active manual status override

→ Sets `status = "Inactive"`.

This ensures cases don't remain Active past their benefit end date.

### `CaseBeneficiaryPeriodSweepHostedService`

Runs on a timer (configurable interval).

Performs two tasks:
1. Expires any timed overrides where `override_status_until <= UtcNow` — clears the override so the next `RefreshAsync` uses computed status.
2. Applies the benefit expiry rule (same as above) as a belt-and-suspenders check.

## Manual Refresh

Case workers can trigger an on-demand lifecycle refresh from the Case Detail view. This is useful after:
- Approving or rejecting the Primary Application outside the normal form approval flow
- Updating benefit period overrides
- Restoring a case from Inactive to Active after extending the programme

The refresh calls `CaseLifecycleService.RefreshAsync` directly and displays a success/warning message in the UI.
