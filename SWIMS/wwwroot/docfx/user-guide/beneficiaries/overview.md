# Beneficiaries — Overview

The **Beneficiaries** module (also referred to as **Clients** in some parts of the system) is the registry of individuals who receive or apply for social welfare services. Every case and most form submissions are linked to a beneficiary record.

## What Is a Beneficiary Record?

A beneficiary record is a profile for an individual that captures:

- Personal details (name, date of birth, gender, marital status)
- Identity document (National ID, Passport, etc.)
- Contact information (telephone, address, city)
- Record status (Active or Archived)

A single person should have **only one beneficiary record** in SWIMS. Always search before creating a new record to avoid duplicates.

## Who Can Do What

| Action | Required Permission |
|--------|-------------------|
| Search and view beneficiary profiles | `Clients.View` |
| Register a new beneficiary | `Clients.Create` |
| Edit a beneficiary's details | `Clients.Edit` |
| Archive a beneficiary record | `Clients.Archive` |

## Related Pages

- [Registering a Beneficiary](registering.md)
- [Searching & Editing](searching-editing.md)
