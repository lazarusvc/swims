# Cases — Overview

A **Case** is the central record that ties a beneficiary's ongoing engagement with a social welfare programme together. It acts as the anchor for everything related to that person's benefit — their application forms, approval outcomes, benefit period, and assigned case worker.

## The Case Lifecycle

```
Case Created (Pending)
      │
      ▼
Primary Application linked → approval workflow runs
      │
      ├─── Approved → Case becomes Active (benefit period starts)
      │
      ├─── Rejected → Case becomes Closed
      │
      └─── Benefit period expires → Case becomes Inactive
```

## Case Statuses

| Status | What it means |
|--------|--------------|
| **Pending** | Case has been created but no Primary Application has been approved yet |
| **Active** | The Primary Application has been approved and the benefit period is current |
| **Inactive** | The benefit period has expired — the case is no longer active but is not closed |
| **Closed** | The application was rejected, or the case was manually closed |

See [Case Status Reference](status-reference.md) for a full breakdown.

## Case Number Format

Each case is assigned a unique reference number in the format: **CASE-YYYY-NNNNN**

For example: `CASE-2025-00042`

Use this number when corresponding about a case.

## Who Can Do What

| Action | Required Permission |
|--------|-------------------|
| View case list and details | `Cases.View` |
| Create cases, link forms, manage assignments | `Cases.Manage` |

## Related Pages

- [Creating a Case](creating.md)
- [Managing a Case](managing.md)
- [Case Status Reference](status-reference.md)
