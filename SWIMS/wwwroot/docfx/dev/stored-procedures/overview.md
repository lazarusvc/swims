# Stored Procedures — Overview

The Stored Procedures module allows administrators to register SQL Server stored procedures in the SWIMS database, configure their parameters, and execute them through a controlled web interface — without direct database access.

## Use Cases

- Running scheduled maintenance or data cleanup procedures
- Generating ad-hoc data extracts
- Triggering migrations or data transformation scripts
- Executing VCAS/SmartStream integration batch jobs

## Data Model

### `StoredProcess`

```csharp
public class StoredProcess
{
    public int Id { get; set; }
    public string Name { get; set; }          // Display name
    public string ProcedureName { get; set; } // Actual SQL proc name (with schema)
    public string? Description { get; set; }
    public string? ConnectionName { get; set; } // Which connection string to use
    public bool IsActive { get; set; }
    public bool ExcludeHeadersOnExport { get; set; } // Omit column headers from CSV export
    public int SortOrder { get; set; }
}
```

### `StoredProcessParam`

```csharp
public class StoredProcessParam
{
    public int Id { get; set; }
    public int StoredProcessId { get; set; }
    public string ParamName { get; set; }     // SQL parameter name (without @)
    public string Label { get; set; }         // UI label
    public string DataType { get; set; }      // "string", "int", "date", "bool"
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public int SortOrder { get; set; }
}
```

## `StoredProcedureRunner`

`StoredProcedureRunner` (singleton) is the execution engine:

```csharp
public class StoredProcedureRunner
{
    Task<StoredProcResult> RunAsync(
        StoredProcess proc,
        IReadOnlyDictionary<string, string> paramValues,
        CancellationToken ct = default);
}
```

It:
1. Opens a connection using the proc's `ConnectionName` (or `DefaultConnection`).
2. Creates a `SqlCommand` with `CommandType.StoredProcedure`.
3. Maps parameter values to typed `SqlParameter` instances using `DataType`.
4. Executes and captures the first result set as a `DataTable`.
5. Returns a `StoredProcResult` containing the data table or any SQL error.

## CSV Export & Header Control

The **Export CSV** button on the results page re-executes the stored procedure and returns the data as a downloadable `.csv` file. By default the first row contains column headers (the `DataTable.Columns` names).

### Excluding headers

Set `ExcludeHeadersOnExport = true` on a `StoredProcess` to omit the header row from the CSV output. This is configured per-procedure via the admin UI (checkbox: *Exclude headers on CSV export*).

Typical use case: **bank file generation** (e.g. SmartStream control-group files) where the downstream system expects raw data rows with no header line.

Implementation: `StoredProcessesController.DataTableToCsv(DataTable dt, bool includeHeaders)` — the `includeHeaders` flag gates the header-writing loop.

## Routes & Permissions

| Route | Permission | Purpose |
|-------|-----------|---------|
| `GET /StoredProcesses` | `SP.Run` | List runnable procedures |
| `GET /StoredProcesses/Run/{id}` | `SP.Run` | Show param form |
| `POST /StoredProcesses/Run/{id}` | `SP.Run` | Execute procedure |
| `GET /StoredProcessesAdmin` | `SP.Manage` | Manage procedure definitions |
| `GET /StoredProcessesAdmin/Create` | `SP.Manage` | New procedure |
| `GET /StoredProcessesAdmin/Edit/{id}` | `SP.Manage` | Edit procedure |
| `GET /StoredProcessParams` | `SP.Params` | Manage procedure parameters |

## Configuration

```json
"StoredProcs": {
  "DefaultConnectionName": "DefaultConnection",
  "MaxRows": 5000,
  "TimeoutSeconds": 120
}
```

## Related Pages

- [Wrappers & Helpers](wrappers.md)
