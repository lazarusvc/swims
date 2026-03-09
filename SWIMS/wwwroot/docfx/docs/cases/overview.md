# Cases — Overview

The Cases module provides structured lifecycle management for a beneficiary's engagement with a social welfare programme. It is a **v1 MVP** that establishes the foundation for full case management (visits, grievances, follow-ups) in future phases.

## What the Module Does (Today)

- Creates and maintains a **case record** tied to a beneficiary and optionally a programme
- Links one or more **form submissions** to a case, with a designatable **Primary Application**
- Computes and maintains a case's **effective status** (`Pending` / `Active` / `Inactive` / `Closed`) driven by the Primary Application's approval outcome and benefit period rules
- Supports **manual overrides** of status and benefit period with guardrails
- Handles case worker **assignment**
- Runs **background sweeps** that auto-mark cases Inactive when benefit periods expire

## What Is Not Yet Covered

- Visits / follow-up logs
- Case notes and narrative history
- Grievance intake and resolution workflows
- Tasking, reminders, SLA tracking

## Case Number Format

Cases are assigned a human-readable identifier on creation: `CASE-YYYY-NNNNN`

- `YYYY` = year of creation
- `NNNNN` = zero-padded 5-digit sequential counter within that year

## `SW_case` Model

```csharp
public class SW_case
{
    public int id { get; set; }
    public string case_number { get; set; }          // "CASE-2025-00001"
    public int? beneficiary_id { get; set; }          // FK → SW_beneficiary
    public int? program_tag_id { get; set; }          // FK → SW_programTag (ref schema)
    public string status { get; set; }                // Pending/Active/Inactive/Closed
    
    // Benefit period (computed or overridden)
    public DateTime? benefit_start_at { get; set; }
    public DateTime? benefit_end_at { get; set; }
    public int? benefit_period_months { get; set; }
    public string? benefit_period_source { get; set; } // "computed" | "override"
    
    // Manual overrides
    public string? override_status { get; set; }
    public DateTime? override_status_until { get; set; }
    public DateTime? override_benefit_start { get; set; }
    public DateTime? override_benefit_end { get; set; }
    public int? override_benefit_months { get; set; }
    
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}
```

## Case Status Values

| Status | Meaning |
|--------|---------|
| `Pending` | No Primary Application, or Primary Application awaiting approval |
| `Active` | Primary Application approved; benefit period current |
| `Inactive` | Benefit period has expired (auto-swept) or manually set |
| `Closed` | Primary Application rejected, or manually closed |

## Routes

| Route | Permission | Purpose |
|-------|-----------|---------|
| `GET /Cases` | `Cases.View` | All cases list |
| `GET /Cases/My` | `Cases.View` | Cases assigned to current user |
| `GET /Cases/Details/{id}` | `Cases.View` | Case detail view |
| `GET /Cases/Create` | `Cases.Manage` | New case form |
| `POST /Cases/Create` | `Cases.Manage` | Save new case |
| `GET /Cases/Edit/{id}` | `Cases.Manage` | Edit case metadata |
| `GET /Cases/Assign/{id}` | `Cases.Manage` | Assign/reassign case worker |
| `GET /Cases/LinkForm/{id}` | `Cases.Manage` | Link a form submission to case |
| `GET /Cases/BenefitPeriod/{id}` | `Cases.Manage` | View/override benefit period |

## Related Pages

- [Lifecycle & Status](lifecycle.md)
- [Benefit Period Management](benefit-period.md)
