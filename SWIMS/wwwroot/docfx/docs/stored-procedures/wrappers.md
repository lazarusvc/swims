# Stored Procedure Wrappers & Helpers

Beyond the admin-managed procedure registry, SWIMS provides a library of **Turnkey Stored Procedures** — pre-built SQL wrappers for common operations — and **Form Helpers** that simplify form data access patterns.

## Turnkey Stored Procedures

Turnkey SPs are stored procedures shipped with SWIMS that cover common data operations. They are pre-registered in the `sp` schema and usable via the Stored Procedures UI.

### Categories

| Category | Description |
|----------|-------------|
| Beneficiary exports | Generate CSV/tabular outputs of beneficiary data filtered by programme, status, date range |
| Case reports | Active cases by programme, expiring benefit lists, case status summaries |
| Payments prep | Pre-export lists for Welfare Form 6/7 and SmartStream integration |
| Audit queries | Cross-entity change summaries for a date range |
| Maintenance | Orphaned record cleanup, index rebuild helpers |

### Naming Convention

All turnkey SPs follow the naming pattern: `sp_swims_<category>_<action>`, e.g.:

- `sp_swims_cases_active_by_programme`
- `sp_swims_payments_export_smartstream`
- `sp_swims_beneficiary_search_advanced`

### Version 3.1 Additions

The v3.1 wrappers (documented in Notion) added:

- **Parameterised date range** support across all report-style SPs
- **Output format flags** (`@format`: `"table"`, `"csv"`, `"summary"`)
- **Row limit parameter** (`@max_rows`) overriding the system default
- Consistent error output shape: `{ error_code, error_message, rows_affected }`

## Form Helpers

Form Helpers (v2) are utility functions in `Services/` and helper classes that simplify working with the EAV form data model.

### `SW_form_FieldAttributes`

```csharp
// Models/ViewModels/form_FieldAttributes.cs
public class form_FieldAttributes
{
    public int FieldId { get; set; }
    public string FieldName { get; set; }
    public string FieldType { get; set; }
    public bool IsRequired { get; set; }
    public string? Options { get; set; }         // JSON array for select/radio
    public string? ConditionalRule { get; set; } // JSON conditional logic
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public string? CurrentValue { get; set; }    // populated for edit/view
}
```

### Common Helper Operations

```csharp
// Get all field attributes for a form submission (for rendering)
var fields = await formHelper.GetFieldsForSubmissionAsync(formId, submissionId);

// Get a specific field value from a submission
var value = await formHelper.GetFieldValueAsync(submissionId, fieldName);

// Get approval-relevant field from a submission (used by CaseLifecycleService)
var decision = formHelper.ExtractApprovalDecision(fields);
// Returns: "Approved" | "Rejected" | null
```

### `ExtractApprovalDecision`

This method defensively checks multiple possible field names in a form submission for an approval decision value, since different form templates may use different field names:

Checked names (in order): `approval_decision`, `decision`, `outcome`, `status`, `approval_status`, `result`.

Values interpreted as **Approved**: `"approved"`, `"yes"`, `"accept"`, `"accepted"`, `"active"`.

Values interpreted as **Rejected**: `"rejected"`, `"no"`, `"deny"`, `"denied"`, `"declined"`, `"closed"`.

This defensiveness is intentional in v1 — a future version will enforce a formal schema contract for approval fields.
