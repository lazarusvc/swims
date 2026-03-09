# Managing Forms (Admin)

*This section is for users with **Forms.Manage** or **Forms.Builder** permissions.*

## Form Definitions vs. Form Submissions

There are two distinct concepts:

- **Form definition** — the template: its fields, layout, approval levels, and programme association. Managed by Form Managers.
- **Form submission** — a completed (or in-progress) instance of that form filled in by a user. Created by anyone with Submit access.

## Creating a New Form Definition

1. Navigate to **Programmes** → click **Manage Forms** (or find the relevant programme and use the admin controls).
2. Click **New Form**.
3. Fill in the form metadata:
   - **Name** — the form's display name
   - **Description** — a brief summary of its purpose
   - **Programme** — which programme this form belongs to
   - **Form Type(s)** — classification (Application, Assessment, Referral, etc.)
4. Click **Save**.

## Configuring Approval Levels

On the form edit screen, scroll to the **Approval Levels** section:

- Toggle each level (L1 through L5) on or off depending on how many approvals this form requires.
- For each active level, you can optionally set a **maximum pending count** — if the approval queue at that level reaches this number, new submissions will be blocked until the queue drains.

The five levels correspond to:

| Level | Typical Role |
|-------|-------------|
| L1 | Social Worker |
| L2 | Coordinator |
| L3 | Director |
| L4 | Permanent Secretary |
| L5 | Minister |

## Building the Form (Field Editor)

Once the form metadata is saved:

1. Click **Edit Form Layout** (or **Form Builder**).
2. The form builder opens with a panel showing the current structure.

### Adding a Table (Group)

A "table" is a logical section within the form (e.g. "Personal Information", "Income Details").

1. Click **Add Section / Table**.
2. Enter a name for the section.
3. Click **Save**.

### Adding Fields

1. Select a table/section.
2. Click **Add Field**.
3. Configure the field:
   - **Label** — the question or field name shown to the user
   - **Field Type** — text, number, date, select, radio, checkbox, textarea, file upload
   - **Required** — whether the field must be completed
   - **Options** — for select/radio/checkbox: the list of choices (one per line)
4. Click **Save Field**.

### Reordering Fields

Drag fields up or down within a section to change their display order.

### Conditional Logic

A field can be set to only show when another field has a specific value:

1. Select the field.
2. Expand **Conditional Logic**.
3. Choose the **controlling field** (must be in the same section).
4. Choose the **condition** (e.g. equals "Yes").
5. Save.

The field will be hidden by default and appear only when the condition is met.

## Linking Forms to Cases

Form submissions can be linked to Cases by case workers. You do not need to configure anything on the form itself for this — the linking is done per-submission by the case worker. However, ensuring the **Form Type** is set correctly helps the case module correctly interpret the submission's role (e.g. "Application" vs. "Assessment").
