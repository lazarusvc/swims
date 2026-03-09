# Reference Data

Reference data provides the lookup values and configuration tables used throughout SWIMS. It is managed in **Admin → Reference Data** (requires `RefData.Manage` permission) and exposed via the `/api/v1` REST endpoints.

## Entities

### Programme Tags (`SW_programTag`)

Programme tags categorise forms and cases into social welfare programmes.

| Field | Description |
|-------|-------------|
| `id` | PK |
| `code` | Short code (e.g., `"PA"`, `"EA65"`) |
| `name` | Display name (e.g., `"Public Assistance"`) |
| `is_active` | Whether the programme is currently accepting applications |
| `default_benefit_months` | Default benefit duration for cases under this programme |
| `sort_order` | Display order in the programme dashboard |

### Form Types (`SW_formType`)

Form types classify a form's role (e.g., "Application", "Assessment", "Referral"). A form can belong to multiple types via the `SW_formFormType` many-to-many table.

### Cities (`SW_city`)

City/district reference table used for beneficiary address fields and organisation addresses.

| Field | Description |
|-------|-------------|
| `id` | PK |
| `name` | City/district name |
| `code` | Short code |

**API**: `GET/POST/PUT/DELETE /api/v1/city`

### Organisations (`SW_organization`)

Organisation registry — government agencies, partner organisations, NGOs. Used in case assignments, form workflows, and reporting.

| Field | Description |
|-------|-------------|
| `id` | PK |
| `name` | Organisation name |
| `type` | Organisation type |
| `city_id` | FK → SW_city |
| `telephone` / `email` | Contact info |

**API**: `GET/POST/PUT/DELETE /api/v1/organization`

### Financial Institutions (`SW_financial_institution`)

Banks and payment providers used in the Payments module for benefit disbursement.

| Field | Description |
|-------|-------------|
| `id` | PK |
| `name` | Institution name |
| `branch` | Branch name/code |
| `city_id` | FK → SW_city |

**API**: `GET/POST/PUT/DELETE /api/v1/financial_institution`

## Admin Routes

| Route | Purpose |
|-------|---------|
| `/ProgramTags` | Manage programme tags |
| `/FormTypes` | Manage form types |
| `/city` | Manage cities |
| `/organization` | Manage organisations |
| `/financial_institution` | Manage financial institutions |

All require `RefData.Manage` permission.

## Lookup Context

Reference data for programme tags and form types is stored in the **`ref` schema** via `SwimsLookupDbContext`. Cities, organisations, and financial institutions remain in the `dbo` schema via `SwimsDb_moreContext`.
