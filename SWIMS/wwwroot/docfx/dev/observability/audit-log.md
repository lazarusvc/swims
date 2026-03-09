# Audit Log

The Audit Log provides a tamper-evident record of all entity changes made through SWIMS. It is populated automatically by `AuditSaveChangesInterceptor` — no manual logging calls are needed in controllers or services.

## What Gets Audited

Any entity type tracked by `SwimsIdentityDbContext` that is configured for auditing. Currently this includes:

- Users (`SwUser`) — creates, profile updates, role changes
- Roles (`SwRole`) — creates, edits, deletes
- Role claims (permission assignments)
- Authorization policies
- Endpoint policy assignments
- Notification routing rules
- System settings changes

> [!NOTE]
> Entities in other DbContexts (Cases, Lookups, etc.) are **not** automatically audited by this interceptor. To audit those, add `AuditSaveChangesInterceptor` to the additional contexts.

## Audit Entry Structure

Each `AuditLog` row captures:

| Field | Description |
|-------|-------------|
| `Timestamp` | UTC timestamp of the change |
| `UserId` / `Username` | Identity of the user who triggered the change |
| `Action` | `"Created"` / `"Modified"` / `"Deleted"` |
| `EntityName` | C# class name of the changed entity |
| `EntityId` | Primary key value (as string) |
| `OldValues` | JSON snapshot of the entity before the change (Modified/Deleted only) |
| `NewValues` | JSON snapshot of the entity after the change (Created/Modified only) |

### Example OldValues/NewValues

```json
// OldValues
{ "IsActive": true, "Name": "Public Assistance" }

// NewValues  
{ "IsActive": false, "Name": "Public Assistance" }
```

Only **changed properties** are included in `OldValues`/`NewValues` for `Modified` entries, not the full entity snapshot. This keeps storage requirements reasonable while highlighting exactly what changed.

## Viewing the Audit Log

**Route**: `GET /Portal/Logs/Audit`  
**Permission**: `Admin.AuditLogs`

### Available Filters

| Filter | Description |
|--------|-------------|
| Username | Free-text search on `Username` |
| Action | Dropdown: Created / Modified / Deleted |
| Entity | Free-text search on `EntityName` |
| From / To | Date range filter on `Timestamp` |

### Export

The audit log view includes a **Export to CSV** button that downloads the current filtered result set as a `.csv` file, suitable for external audit review or compliance reporting.

## Performance Considerations

Audit log writes occur in the **same database transaction** as the data change. This ensures consistency but adds a small overhead to every write operation on audited entities.

For high-write scenarios (bulk imports, background sweeps), consider:

- Disabling audit on the specific DbContext save call using a flag
- Writing audit rows to a separate async queue (future enhancement)
- Archiving old audit rows to cold storage on a schedule

## Retention

There is no automatic retention/purge policy in v1. Implement a scheduled stored procedure or Hangfire job to archive or delete rows older than your retention target (e.g., 3 years for government compliance).
