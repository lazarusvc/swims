# Approvals Workflow

SWIMS implements a **configurable five-level sequential approval workflow**. Each form definition controls which levels are active and the maximum pending count per level.

## Approval Levels

The five levels map to the Government of Dominica's typical social welfare approval chain:

| Level | Permission | Typical Role |
|-------|-----------|-------------|
| L1 | `Approvals.Level1` | Social Worker |
| L2 | `Approvals.Level2` | Coordinator |
| L3 | `Approvals.Level3` | Director |
| L4 | `Approvals.Level4` | Permanent Secretary |
| L5 | `Approvals.Level5` | Minister |

A form can have any combination of levels active. For example, a simple internal referral might only use L1 and L2, while a high-value programme might require all five.

## Approval Flow

```
Form Submitted
      │
      ▼
  L1 Pending  ─── L1 Approve ──▶  L2 Pending  ─── L2 Approve ──▶ ... ──▶ Final Approved
      │                                 │
  L1 Reject                         L2 Reject
      │                                 │
      ▼                                 ▼
   Rejected                          Rejected
```

- Rejection at any level ends the workflow immediately with status **Rejected**.
- Approval at the last active level marks the submission as **Approved**.
- If a level is not active (`isApproval_XX = false`), it is skipped automatically.

## `SW_formProcess` Model

Each approval action creates or updates a `SW_formProcess` row:

```csharp
public class SW_formProcess
{
    public int id { get; set; }
    public int form_id { get; set; }         // submission being processed
    public int approval_level { get; set; }   // 1–5
    public string? status { get; set; }       // "Pending", "Approved", "Rejected"
    public int? processed_by { get; set; }    // userId
    public DateTime? processed_at { get; set; }
    public string? notes { get; set; }
}
```

## Pending Count Guard

To prevent a backlog from blocking approvers:

- Each form level can have a `approval_level_0X` integer cap.
- Before a submission is accepted into the queue, `ApprovalsController` counts pending items at that level for this form.
- If `count >= cap`, the submission is blocked with an `ApprovalBlocked` error view and the user is told to wait.

## Approvals Dashboard

**Route**: `GET /Approvals` (requires `Approvals.View`)

The dashboard shows the current user's pending approval queue — grouped by form and filtered to the levels the user has permission for. Clicking a queue item navigates to `form/Approval/{submissionId}` for review.

Pending counts are shown per form per level, giving approvers visibility into workload.

## Approval Action

**Route**: `POST /form/ApprovalAction/{id}` (requires the appropriate `Approvals.Level*` permission)

The controller:

1. Loads the `SW_formProcess` record for the current level.
2. Validates that the submitting user holds the correct level permission.
3. Records the decision (approve/reject), approver user ID, timestamp, and optional notes.
4. If approved and a next level exists (and is active), creates the next level's pending record.
5. If approved and no next level, marks the submission as fully approved and triggers lifecycle events (e.g., case status refresh via Elsa workflow or direct `CaseLifecycleService` call).

## Integration with Cases

When a form submission is linked to a Case as the **Primary Application**, full approval of that submission triggers `ICaseLifecycleService.RefreshAsync(caseId)`, which recalculates the case status and benefit period based on the new approval outcome.
