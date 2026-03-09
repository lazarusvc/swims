# Conditional Logic

SWIMS supports **per-field conditional visibility rules** in form submissions. A field can be configured to show or hide based on the value of another field in the same form.

## Rule Format

Each `SW_formTableData_Type.conditional_rule` stores a JSON object:

```json
{
  "show_if": {
    "field_id": 42,
    "operator": "equals",
    "value": "Yes"
  }
}
```

Or with multiple conditions:

```json
{
  "show_if": {
    "logic": "any",
    "conditions": [
      { "field_id": 42, "operator": "equals", "value": "Yes" },
      { "field_id": 43, "operator": "not_empty" }
    ]
  }
}
```

## Supported Operators

| Operator | Description |
|---------|-------------|
| `equals` | Field value exactly matches |
| `not_equals` | Field value does not match |
| `contains` | Field value contains string |
| `not_empty` | Field has a non-blank value |
| `empty` | Field has no value |
| `greater_than` | Numeric comparison |
| `less_than` | Numeric comparison |

## Logic Combinators

- `"logic": "all"` — all conditions must be true (AND)
- `"logic": "any"` — at least one condition must be true (OR)

Default when no `logic` key is present: single condition evaluation.

## Client-Side Evaluation

Conditional logic is evaluated in the browser by `conditional-logic.js` (served from `wwwroot/js/`):

- On page load, all fields with `conditional_rule` are initially hidden or shown according to current field values.
- On any field `change` event, all rules are re-evaluated and affected fields are shown/hidden with a smooth transition.
- Hidden fields have their `required` attribute removed to prevent HTML5 validation errors on submit.

## Server-Side Validation

The form submission controller re-evaluates conditional rules server-side before saving. Fields that are conditionally hidden based on the submitted values are excluded from required-field validation and not persisted (or saved with a null value), ensuring data integrity.

## Configuring Rules in the Builder

In the [Form Builder](form-builder.md):

1. Select a field in the editor panel.
2. Expand the **Conditional Logic** section.
3. Choose a **controlling field** from the same form.
4. Select the **operator** and enter the **trigger value**.
5. Add additional conditions if needed.
6. Save the field.

The rule is stored in `conditional_rule` and takes effect immediately for new submissions.

## Current Limitations

- Cross-table conditional rules are **not supported** — the controlling and dependent fields must be in the same form table.
- Circular dependencies (field A controls B, B controls A) are not detected at configuration time — avoid these configurations.
- Rules apply only to the submission UI; the builder preview does not evaluate live conditional logic.
