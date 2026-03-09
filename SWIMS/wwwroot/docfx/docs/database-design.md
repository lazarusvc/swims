# Database Design

SWIMS uses a **single SQL Server database** partitioned into logical schemas. Each schema is owned by a dedicated EF Core DbContext with its own migration history.

## Schema Map

```
Database: SWIMS
│
├── auth.*          Identity users, roles, claims, tokens
├── notify.*        Notifications, push subscriptions, preferences, routing
├── msg.*           Conversations, conversation members, messages
├── ops.*           Hangfire jobs + Audit log + Session log
├── ac.*            Access control (endpoint policies, public endpoints, auth policies)
├── case.*          Cases, case-form links, case assignments
├── ref.*           Lookup/reference data (program tags, form types, links)
├── rpt.*           Report definitions and params
├── sp.*            Stored procedure registry + params
└── dbo.*           Core forms, beneficiaries, cities, organisations, financial institutions
```

## Core Entity Relationships

### Forms & Approvals

```
SW_form (dbo)
  ├── form_id (PK)
  ├── program_tag (FK → SW_programTag.id)
  ├── form_type_id (FK → SW_formType.id)
  ├── isApproval_01..05  — which approval levels are active
  ├── approval_level_01..05 — max pending count guard per level
  └── * field data stored as SW_formTableDatum rows

SW_formProcess (dbo)
  └── tracks approval-level progress per submission

SW_formReport (dbo)
  └── links a form submission to an SSRS report
```

### Cases

```
SW_case (case schema)
  ├── id (PK)
  ├── case_number  "CASE-YYYY-NNNNN"
  ├── beneficiary_id (FK → SW_beneficiary.id)
  ├── program_tag_id (FK → SW_programTag.id)
  ├── status  {Pending, Active, Inactive, Closed}
  ├── benefit_start_at / benefit_end_at / benefit_period_months
  ├── override_status / override_status_until    — manual override fields
  ├── override_benefit_* fields                  — manual period overrides
  └── created_at / updated_at

SW_caseForm (case schema)
  ├── case_id (FK → SW_case.id)
  ├── form_id (FK → form submission)
  ├── role_label
  ├── is_primary_application  — only one per case
  └── linked_at / linked_by

SW_caseAssignment (case schema)
  ├── case_id
  └── assigned_to_user_id
```

### Beneficiaries

```
SW_beneficiary (dbo)
  ├── id / uuid
  ├── first_name / middle_name / last_name
  ├── dob / gender / marital_status
  ├── id_number / id_type
  ├── telephone / email
  └── status / created_at
```

### Reference Data

```
SW_programTag (ref schema)
  ├── id / code / name
  ├── is_active
  ├── default_benefit_months    — default benefit duration for new cases
  └── sort_order

SW_formType (ref schema)
  ├── id / name
  └── is_active

SW_formFormType (ref schema)
  └── many-to-many: SW_form ↔ SW_formType

SW_formProgramTag (ref schema)
  └── many-to-many: SW_form ↔ SW_programTag
```

### Identity & Security

```
SwUser : IdentityUser<int> (auth schema)
  ├── standard Identity columns
  ├── IsLdapUser (bool)
  ├── LdapUsername
  └── LdapDomain

SwRole : IdentityRole<int> (auth schema)

AccessControl (ac schema)
  ├── EndpointPolicy — named policy with claim requirements
  ├── EndpointPolicyAssignment — maps controller/action → policy
  └── PublicEndpoint — routes that bypass authentication

AuthPolicies (ac schema)
  └── DB-stored policy definitions (evaluated by DbAuthorizationPolicyProvider)
```

### Notifications

```
notify.notifications
  ├── Id (GUID PK)
  ├── UserId / Username
  ├── Type (e.g. "NewMessage")
  ├── PayloadJson (event-specific JSON)
  ├── Seen (bit)
  └── CreatedUtc

notify.push_subscriptions
  ├── Id / UserId
  ├── Endpoint / P256dh / Auth (VAPID keys)
  ├── IsActive
  └── CreatedUtc / LastSeenUtc

notify.notification_preferences
  ├── UserId / Type
  └── InApp / Email / Digest (override flags)

notify.notification_routing
  └── per-type routing rules (overrides global defaults for email/push)
```

### Messaging

```
msg.conversations
  └── Id (GUID) / CreatedAt / LastMessageAt

msg.conversation_members
  └── ConversationId / UserId / JoinedAt / LastReadAt

msg.messages
  └── Id / ConversationId / SenderId / Body / CreatedAt / IsDeleted
```

### Observability

```
ops.audit_logs
  ├── Id / Timestamp
  ├── UserId / Username
  ├── Action / EntityName / EntityId
  └── OldValues / NewValues (JSON diffs via EF interceptor)

ops.session_logs
  ├── Id / UserId / Username
  ├── LoginAt / LogoutAt
  ├── IpAddress / UserAgent
  └── IsActive
```

## EF Audit Interceptor

`AuditSaveChangesInterceptor` hooks into `SwimsIdentityDbContext.SaveChangesAsync` to capture `Added`, `Modified`, and `Deleted` entity state changes and write rows to `ops.audit_logs` automatically. No manual audit calls are needed in controllers — the interception is transparent.

## Seed Data

`Data/SeedData.cs` runs on startup in Development (or when `Auth:SeedOnStartup = true`) and provides:

- Default roles (Admin, ProgramManager, CaseWorker, Approver, Viewer, etc.)
- Default admin user
- Base reference data (program tags, form types)
- Default system settings

Seeding is **idempotent** — safe to run multiple times.
