# Beneficiaries

The Beneficiaries module (referred to internally as **Clients**) manages the registry of individuals receiving or applying for social welfare services.

## Model: `SW_beneficiary`

```csharp
public class SW_beneficiary
{
    public int id { get; set; }
    public Guid uuid { get; set; }
    
    // Name
    public string? first_name { get; set; }
    public string? middle_name { get; set; }
    public string? last_name { get; set; }
    
    // Identity
    public DateTime? dob { get; set; }
    public string? gender { get; set; }
    public string? marital_status { get; set; }
    public string? id_number { get; set; }
    public string? id_type { get; set; }       // e.g. "National ID", "Passport"
    
    // Contact
    public string? telephone { get; set; }
    public string? email { get; set; }
    public string? address { get; set; }
    public int? city_id { get; set; }           // FK → SW_city
    
    // Status
    public string? status { get; set; }         // "Active", "Archived"
    
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
}
```

## Routes

| Route | Permission | Purpose |
|-------|-----------|---------|
| `GET /beneficiary` | `Clients.View` | List all beneficiaries |
| `GET /beneficiary/Details/{id}` | `Clients.View` | View beneficiary profile |
| `GET /beneficiary/Create` | `Clients.Create` | New beneficiary form |
| `POST /beneficiary/Create` | `Clients.Create` | Save new beneficiary |
| `GET /beneficiary/Edit/{id}` | `Clients.Edit` | Edit beneficiary |
| `POST /beneficiary/Edit/{id}` | `Clients.Edit` | Save edits |
| `GET /beneficiary/Delete/{id}` | `Clients.Archive` | Confirm archive/delete |
| `POST /beneficiary/Delete/{id}` | `Clients.Archive` | Archive beneficiary |

## UUID vs Integer ID

Beneficiaries have both an integer `id` (used as FK in cases and form submissions) and a `uuid` (GUID). The UUID is intended for external system integrations (e.g., VCAS, SmartStream) where sharing sequential integer IDs may not be desirable.

## Linking to Cases

A beneficiary can have multiple cases (one per programme or period). When creating a case, the user selects the beneficiary by searching the registry. See [Cases — Overview](../cases/overview.md).

## API Access

Beneficiaries are also exposed via the REST API:

```
GET  /api/v1/beneficiary        List with optional ?q= search
GET  /api/v1/beneficiary/{id}   Get by ID
POST /api/v1/beneficiary        Create
PUT  /api/v1/beneficiary/{id}   Update
```

Requires `Api.Access` permission.
