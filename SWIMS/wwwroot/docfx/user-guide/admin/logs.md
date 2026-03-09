# Audit & Session Logs

## Audit Log

*Requires `Admin.AuditLogs` permission.*

Navigate to **Admin → Audit Log** (or **Portal → Logs → Audit**).

The Audit Log is an automatically maintained record of every change made to important data in SWIMS — who changed what, when, and what the values were before and after.

### What Is Logged

- User account changes (creation, profile edits, role assignments)
- Role changes (creation, permission additions/removals)
- Authorization policy changes
- Endpoint policy assignments
- System settings changes
- Notification routing changes

### Reading the Audit Log

Each row in the audit log shows:

| Column | Description |
|--------|-------------|
| **Timestamp** | When the change was made (UTC) |
| **User** | Which user made the change |
| **Action** | Created, Modified, or Deleted |
| **Entity** | What type of record was changed (e.g. SwUser, SwRole) |
| **Entity ID** | The record's identifier |
| **Old Values** | What the values were before the change |
| **New Values** | What the values became after the change |

### Filtering

Use the filter controls at the top to narrow by:

- **Username** — find changes by a specific user
- **Action** — show only Created, Modified, or Deleted records
- **Entity** — filter by record type
- **Date range** — narrow to a specific period

### Exporting

Click **Export CSV** to download the current filtered results as a spreadsheet for external audit review.

---

## Session Log

*Requires `Admin.SessionLogs` permission.*

Navigate to **Admin → Session Log** (or **Portal → Logs → Sessions**).

The Session Log records every login and logout event in the system.

### What Is Logged

| Event | Recorded data |
|-------|--------------|
| **Login** | Username, timestamp, IP address, browser/device |
| **Logout** | Timestamp of sign-out |

### Active Sessions

Rows where **Logout At** is blank and the session is marked **Active** represent currently logged-in users. This is useful for security monitoring — if you see an active session you do not recognise, escalate to your security administrator.

> [!NOTE]
> If a user closes their browser without logging out, their session will remain shown as "active" until the session naturally expires. This is normal.

### Filtering

Filter by username, active-only, or date range.
