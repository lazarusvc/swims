# Benefit Period Management

The benefit period defines how long an approved case's benefits are active: `benefit_start_at` through `benefit_end_at`.

## How the Period Is Computed

`CaseLifecycleService.RefreshAsync` applies a **priority hierarchy** when determining the effective period:

```
1. Case-level override?
   ├── override_benefit_start → use as start
   ├── override_benefit_end   → use as end (skip months calculation)
   └── override_benefit_months → use for duration if no explicit end

2. No override → derive from Primary Application
   ├── Pull start from approval date fields (fallback to UtcNow)
   ├── Pull explicit end if available in form data
   └── Calculate end = start + months (source hierarchy below)

Months source hierarchy:
   Case override_benefit_months
     → Form-level default months
       → Programme Tag default_benefit_months
         → Hardcoded fallback (e.g. 12 months)
```

A `benefit_period_source` string is stamped on the case (`"computed"` or `"override"`) so you can tell at a glance where the effective period came from.

## Benefit Period Override UI

**Route**: `GET /Cases/BenefitPeriod/{id}` (requires `Cases.Manage`)

The Benefit Period screen lets case workers:

| Action | Effect |
|--------|--------|
| View effective values | Shows `benefit_start_at`, `benefit_end_at`, `benefit_period_months`, `benefit_period_source` |
| Save start/end override | Sets `override_benefit_start` / `override_benefit_end`, triggers `RefreshAsync` |
| Save months override | Sets `override_benefit_months`, triggers `RefreshAsync` |
| Clear override | Removes `override_benefit_*` values, triggers `RefreshAsync` (resets to computed) |

After saving or clearing, `RefreshAsync` is called immediately so the displayed case status reflects the change.

## Programme Tag Default Months

Each `SW_programTag` has a `default_benefit_months` integer. This is the fallback when no other months source is available. Configure it in **Admin → Reference Data → Programme Tags**.

## Effect on Case Status

The benefit period is directly tied to case status:

- `benefit_end_at` in the future → case eligible to remain/become `Active`
- `benefit_end_at` in the past and no override → background sweep sets status to `Inactive`
- Override present → status follows the override regardless of computed period

## `CaseBenefitPeriodEditViewModel`

The Benefit Period edit screen uses this view model:

```csharp
public class CaseBenefitPeriodEditViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; }
    
    // Current effective values (read-only display)
    public DateTime? EffectiveStart { get; set; }
    public DateTime? EffectiveEnd { get; set; }
    public int? EffectiveMonths { get; set; }
    public string? Source { get; set; }
    
    // Override inputs
    public DateTime? OverrideStart { get; set; }
    public DateTime? OverrideEnd { get; set; }
    public int? OverrideMonths { get; set; }
}
```
