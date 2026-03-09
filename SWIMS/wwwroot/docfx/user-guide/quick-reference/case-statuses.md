# Case Status Definitions

Quick reference card for all case status values in SWIMS.

| Status | Colour | Meaning | How to get out of it |
|--------|--------|---------|---------------------|
| **Pending** | 🟡 Yellow | Case created; Primary Application not yet approved | Link a form as Primary Application and complete the approval workflow |
| **Active** | 🟢 Green | Primary Application approved; benefit period current | Benefit period expires (auto → Inactive) or manual Close |
| **Inactive** | ⚪ Grey | Benefit period has expired | Extend the benefit period or set an Active override |
| **Closed** | 🔴 Red | Application rejected or manually closed | Requires new application; contact your manager |

## Status Transition Rules

```
Created ──────────────────────────────► Pending
                                            │
                        Primary Application approved
                                            │
                                            ▼
                        benefit end in future ──► Active
                        benefit end past ─────► Inactive
                                            │
                        Primary Application rejected
                                            │
                                            ▼
                                          Closed

Active ──────────────────────────────────────► Inactive (automatic, overnight)
       benefit end date passes, no override

Any status ──────────────────────────────────► Any status (manual override)
```

## Manual Override Notes

- A **manual status override** bypasses all computed status logic while it is active.
- Overrides can be set with or without an expiry date.
- Once an override expires (or is manually cleared), the computed status is restored on the next refresh.
- Overrides are visible in the Case Detail view with an indicator badge.
