# Reference Data — Overview

*This section is for users with the **RefData.Manage** permission — typically administrators and programme managers.*

**Reference Data** contains the lookup tables that power dropdown menus and selection lists throughout SWIMS. Keeping this data accurate and up to date ensures that staff see the correct options when registering beneficiaries, creating cases, and filling in forms.

## What's Included

| Reference Type | Where it's used |
|---------------|----------------|
| **Programme Tags** | The list of social welfare programmes shown on the Programme Dashboard and in case creation |
| **Form Types** | Classifications applied to form definitions (Application, Assessment, Referral, etc.) |
| **Cities / Districts** | Dropdown for beneficiary address fields and organisation locations |
| **Organisations** | Government agencies and partner organisations used in workflows and reporting |
| **Financial Institutions** | Banks and payment providers for the Payments module |

## Managing Reference Data

Each reference type is accessible from the sidebar (or via the Admin area, depending on your layout):

- **Cities**: `/city`
- **Organisations**: `/organization`
- **Financial Institutions**: `/financial_institution`
- **Programme Tags**: `/ProgramTags`
- **Form Types**: `/FormTypes`

On each list page you can:

1. **Create** — add a new entry using the **New** button
2. **Edit** — click an entry to update its details
3. **Delete** — remove an entry (only if it is not in use)

## Programme Tags — Special Fields

Programme Tags have two additional fields important for case management:

| Field | Purpose |
|-------|---------|
| **Default Benefit Months** | The default benefit duration for cases under this programme. Used when no other duration source is available during lifecycle calculation. |
| **Sort Order** | Controls the display order of programmes on the Programme Dashboard. Lower numbers appear first. |
| **Is Active** | Inactive programmes are hidden from the dashboard and cannot be selected for new cases. |

> [!TIP]
> Before creating a new Programme Tag, confirm with your administrator that the programme has been formally established. Programme Tags drive significant downstream data — cases, form links, and reporting all depend on them.
