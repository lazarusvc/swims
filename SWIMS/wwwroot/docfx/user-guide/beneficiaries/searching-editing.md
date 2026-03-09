# Searching & Editing Beneficiaries

## Searching for a Beneficiary

Navigate to **Beneficiaries** in the sidebar. The list shows all registered beneficiaries.

### Search Bar

Use the search bar at the top of the list. You can search by:

- **Name** — first name, last name, or a combination
- **ID Number** — enter the full or partial ID number
- **Case Number** — find the beneficiary linked to a specific case

Type at least 3 characters to trigger results. Results update as you type.

### Filters

Use the filter controls to narrow the list by:

- **Status** — Active or Archived
- **City / District** — filter by location
- **Date Registered** — find records created within a date range

## Viewing a Beneficiary Profile

Click any beneficiary's name or the **View** button to open their profile. The profile shows:

- All personal and contact details
- Their ID document information
- A list of **linked cases** (with status and programme)
- The record's creation date and last updated date

## Editing a Beneficiary Record

1. Open the beneficiary profile.
2. Click **Edit**.
3. Update the relevant fields.
4. Click **Save**.

> [!NOTE]
> All edits are logged in the system audit trail. Ensure changes are accurate — especially ID numbers and dates of birth, which may affect eligibility and payments.

### What Can Be Changed

All fields on the registration form can be updated, including name, date of birth, identity document, and contact information.

### What Cannot Be Changed

The system-assigned beneficiary ID (integer and UUID) cannot be modified. If a record was created with a fundamental error (e.g. a completely wrong person's details), contact your administrator rather than editing over the existing record.

## Archiving a Beneficiary Record

Archiving marks a beneficiary as inactive without deleting their history. This is appropriate when a beneficiary is deceased, has emigrated, or is otherwise no longer eligible.

1. Open the beneficiary profile.
2. Click **Archive** (or **Delete** — depending on configuration, this may archive rather than permanently delete).
3. Confirm the action.

> [!WARNING]
> Archiving does not affect existing linked cases — those remain accessible. However, the beneficiary will no longer appear in the default list view and cannot be selected for new cases or submissions.

## Viewing Linked Cases

On the beneficiary profile, the **Cases** section lists all cases linked to this person, including their status, programme, and case number. Click any case number to navigate directly to that case.
