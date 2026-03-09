# Form Builder

The Form Builder provides a visual, drag-and-drop interface for constructing data capture forms without writing code. It is accessible via **Forms → Edit Upload** (requires `Forms.Builder` permission).

## Concepts

### Form Tables

A form is composed of one or more **tables** (`SW_formTableName`). Each table represents a logical grouping of fields (e.g., "Personal Information", "Household Members", "Income Details").

### Field Types

Each field within a table is defined by a `SW_formTableData_Type` row:

| Property | Description |
|----------|-------------|
| `name` | Field label |
| `field_type` | Input type: `text`, `number`, `date`, `select`, `radio`, `checkbox`, `textarea`, `file`, etc. |
| `is_required` | Whether the field must be filled |
| `options` | JSON array of options (for `select`/`radio`/`checkbox`) |
| `sort_order` | Display order within the table |
| `is_visible` | Default visibility |
| `conditional_rule` | JSON conditional logic rule (see [Conditional Logic](conditional-logic.md)) |

### Submissions

When a user fills out and submits a form, each field value is persisted as a `SW_formTableDatum` row:

```
SW_formTableDatum
├── form_id         (which form definition)
├── table_name_id   (which table in the form)
├── field_type_id   (which field definition)
├── value           (the actual data entered)
├── submitted_by
└── submitted_at
```

## Builder UI

The builder page (`/form/EditUpload/{id}`) renders an interactive editor backed by JavaScript:

- **Left panel**: form structure tree — tables and their fields
- **Right panel**: field editor — properties for the selected field
- **Add Table**: creates a new `SW_formTableName` group
- **Add Field**: creates a `SW_formTableData_Type` under the selected table
- **Drag to reorder**: updates `sort_order` values
- **Delete**: removes a field or table (only if no submissions reference it)

Changes are saved via AJAX calls to the form/table/field CRUD endpoints.

## Linking Forms to Programmes and Types

From the Form metadata edit screen (`/form/edit/{id}`):

- **Programme Tag**: associates the form with a programme (e.g., "Public Assistance"). Controls which dashboard section the form appears in.
- **Form Types**: many-to-many via `SW_formFormType`. Form types classify the form's purpose (e.g., "Application", "Assessment", "Review") and influence how the case module interprets form submissions.

## Approval Levels

From the same form metadata screen, each of the five approval levels can be toggled on/off (`isApproval_01` to `isApproval_05`). For active levels, a **max pending count** can be set — if there are more pending submissions at that level than the cap, new submissions for that form are blocked until the queue drains.

## Form Linking in Cases

A form submission can be linked to a Case. The `is_primary_application` flag on the link marks one submission as the definitive application driving case status and benefit period. See [Cases — Overview](../cases/overview.md).
