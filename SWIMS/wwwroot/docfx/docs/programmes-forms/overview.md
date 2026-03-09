# Programmes & Forms — Overview

The Programmes & Forms module is the core of SWIMS data collection. It provides:

- **Programme Tags** — categories that group related forms (e.g., "Public Assistance", "Elderly Assistance 65+")
- **Form Management** — CRUD for form templates with metadata, approval configuration, and programme/type associations
- **Form Builder** — a visual drag-and-drop field builder for constructing data capture forms
- **Conditional Logic** — show/hide fields based on other field values
- **Form Submission** — end-users complete and submit forms
- **Approvals Workflow** — up to five sequential approval levels with a configurable pending count guard per level

## Key Models

### `SW_form`

```csharp
// Models/SW_form.cs
public class SW_form
{
    public int id { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
    public int? form_type_id { get; set; }         // FK → SW_formType
    public string? program_tag { get; set; }        // FK → SW_programTag.code
    public bool isApproval_01 { get; set; }         // L1 active?
    public bool isApproval_02 { get; set; }         // L2 active?
    public bool isApproval_03 { get; set; }
    public bool isApproval_04 { get; set; }
    public bool isApproval_05 { get; set; }
    public int? approval_level_01 { get; set; }    // max pending before level 1 is blocked
    // ...through approval_level_05
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
}
```

### `SW_formTableDatum` / `SW_formTableName` / `SW_formTableData_Type`

Form field data is stored in a **EAV (Entity-Attribute-Value)** style:

- `SW_formTableName` — defines the table/group name for a form's fields
- `SW_formTableData_Type` — defines a field type within a form table
- `SW_formTableDatum` — stores the actual field value for a specific submission

### `SW_formProcess`

Tracks the approval state of a submission — which level it's currently at, who approved/rejected, and timestamps.

## Routes

| Route | Permission | Purpose |
|-------|-----------|---------|
| `GET /form` | `Programs.View` | Programme dashboard (all forms grouped by programme) |
| `GET /form/Program/{id}` | `Programs.View` | Expanded programme view |
| `GET /form/create` | `Forms.Manage` | New form metadata |
| `POST /form/create` | `Forms.Manage` | Save new form |
| `GET /form/edit/{id}` | `Forms.Manage` | Edit form metadata |
| `GET /form/EditUpload/{id}` | `Forms.Builder` | Form builder (visual editor) |
| `GET /form/Preview/{id}` | `Forms.Access` | Preview a form layout |
| `GET /form/Update/{id}` | `Forms.Submit` | Submit/fill a form |
| `GET /form/Approval/{id}` | `Approvals.View` | View approval state of a submission |
| `POST /form/ApprovalAction/{id}` | `Approvals.Level*` | Approve / reject at a level |
| `GET /form/Complete/{id}` | `Forms.Access` | View completed/submitted form |

## Programme Dashboard

The **Programme Dashboard** (`/form` → `form/Program`) shows all active programme tags and their associated forms. Forms are grouped by programme tag. Users with only `Programs.View` can browse and view submitted forms but cannot build or manage them.

## Related Pages

- [Form Builder](form-builder.md)
- [Conditional Logic](conditional-logic.md)
- [Approvals Workflow](approvals.md)
