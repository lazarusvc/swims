# Glossary

Key terms used in SWIMS and this guide.

---

**Active Directory (AD)**
A Microsoft network directory service used for centralised authentication. Staff with AD accounts log in to SWIMS using their normal Windows username and password.

---

**Approval Level (L1–L5)**
One step in the form approval workflow. A form may require between one and five sequential approval decisions before it is considered fully approved. Each level corresponds to a role in the organisational hierarchy (e.g. L1 = Social Worker, L5 = Minister).

---

**Audit Log**
A system-maintained record of all changes to important records (users, roles, settings). Used for accountability and compliance review.

---

**Benefit Period**
The span of time during which a beneficiary is entitled to receive assistance under a programme. Defined by a start date and an end date. When the benefit period expires, the case automatically moves to **Inactive**.

---

**Beneficiary / Client**
An individual registered in SWIMS as a recipient or applicant for social welfare services.

---

**Case**
The central record linking a beneficiary to a specific social welfare programme engagement. Contains the linked application form, approval status, benefit period, and case worker assignment.

---

**Case Number**
The unique identifier assigned to each case, in the format `CASE-YYYY-NNNNN` (e.g. `CASE-2025-00042`).

---

**Conditional Logic**
A feature of form fields where a field's visibility depends on the value of another field. If the condition is not met, the field is hidden.

---

**Daily Digest**
A single summary email sent each morning at 8:00 AM listing all unread notifications from the previous day. A quieter alternative to immediate notification emails.

---

**DocFX**
The documentation platform used to generate and host SWIMS documentation as a static website.

---

**Endpoint Policy**
An authorization rule mapped to a specific URL route in the application, controlling who can access it without requiring code changes.

---

**Form Definition**
The template for a form — its fields, layout, sections, and approval level configuration. Managed by users with `Forms.Manage` or `Forms.Builder` permissions.

---

**Form Submission**
A completed (or in-progress) instance of a form definition, filled in by a user for a specific beneficiary.

---

**Hangfire**
The background job processing system used by SWIMS to run scheduled and queued tasks (email sending, notification delivery, case maintenance).

---

**LDAP**
Lightweight Directory Access Protocol — the standard used by SWIMS to authenticate users against Active Directory.

---

**Local Account**
A SWIMS account created directly in the system (not linked to Active Directory). Identified by email address for login.

---

**Notification Routing**
The system-level configuration controlling which delivery channels (in-app, email, push, digest) are active for each notification type.

---

**Permission**
A named access right that controls whether a user can perform a specific action (e.g. `Cases.Manage`, `Forms.Submit`). Permissions are bundled into **Roles**.

---

**Primary Application**
The form submission linked to a case that drives the case's status and benefit period. Only one Primary Application can be active per case at a time.

---

**Programme**
A social welfare initiative (e.g. "Public Assistance", "Elderly Assistance 65+"). Each programme is associated with one or more form definitions.

---

**Programme Tag**
The reference data record that defines a programme — its name, default benefit duration, sort order, and active status.

---

**Push Notification**
A native browser or device notification delivered even when SWIMS is not open in a browser tab. Requires a one-time permission grant in the browser.

---

**PWA (Progressive Web App)**
A feature that allows SWIMS to be installed on a device as an app (via "Add to Home Screen" or the browser install prompt), enabling offline-capable features and push notifications.

---

**Reference Data**
Lookup tables that power dropdown menus throughout SWIMS — cities, organisations, financial institutions, programme tags, and form types.

---

**Role**
A named collection of permissions assigned to users. For example, a "CaseWorker" role might include `Cases.Manage`, `Clients.View`, and `Forms.Submit`.

---

**SignalR**
The real-time communication technology used by SWIMS for the Messenger chat and live notification delivery.

---

**SSRS (SQL Server Reporting Services)**
The Microsoft reporting platform that powers SWIMS reports. Reports are generated server-side and proxied through SWIMS to the user's browser.

---

**Status Override**
A manually set case status that takes precedence over the system-computed status. Can be time-limited with an expiry date.

---

**SWIMS**
Social Welfare Information Management System — the digital platform built for the Government of the Commonwealth of Dominica in partnership with the World Food Programme.

---

**TOTP (Time-based One-Time Password)**
The standard used for two-factor authentication codes. A 6-digit code that changes every 30 seconds, generated by an authenticator app.

---

**Two-Factor Authentication (2FA)**
An additional login security layer requiring a time-based code from an authenticator app after the normal username/password step.
