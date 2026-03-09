# Case Status Reference

## Status Definitions

### Pending

The case has been created but is not yet active. This is the starting status for every new case.

**Reasons a case stays Pending:**
- No form submission has been linked as the Primary Application yet
- A Primary Application is linked but is still awaiting approval (moving through the approval levels)
- The Primary Application approval status cannot be determined from the submission data

**What to do:** Ensure a form submission is linked as Primary Application and that it proceeds through the approval workflow.

---

### Active

The Primary Application has been **approved** at all required levels, and the benefit period has not yet expired.

**What this means:** The beneficiary is currently receiving the benefit associated with this programme. The benefit period (start date → end date) is shown on the case detail.

**Automatic transition out of Active:**
- When the benefit end date passes with no active status override, the system automatically sets the status to **Inactive** (runs nightly).

---

### Inactive

The benefit period has **expired** — the case was Active but the end date has now passed, or the case was manually set to Inactive.

**This is not the same as Closed.** An Inactive case can be re-activated by:
- Extending the benefit period (see [Managing a Case — Benefit Period](managing.md))
- Setting a new Primary Application with an updated approval
- Manually overriding the status to Active with a new override-until date

---

### Closed

The case is definitively ended.

**Reasons for Closed:**
- The Primary Application was **rejected** at any approval level
- The case was manually set to Closed by a case manager

**Important:** A Closed case should generally remain closed. To reopen, a new application/form submission would typically be linked as the Primary Application.

---

## Computed vs. Override Status

SWIMS computes case status automatically based on the Primary Application's approval outcome and the benefit period. However, a **manual status override** can be set by authorised users.

| | Computed | Override Active |
|-|---------|----------------|
| **Who sets it** | System (automatic) | Case manager (manual) |
| **When it applies** | Always, unless override is active | While override is set and not expired |
| **Expires** | Never (always current) | On the Override Until date, or when cleared |
| **Shown in UI** | Default status badge | Orange "Override" badge |

The override **always wins** while it is active. Once it expires (or is manually cleared), the computed status takes over on the next refresh.
