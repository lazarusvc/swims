# Managing Users

*Requires `Admin.Users` permission.*

## User List

Navigate to **Admin → Users** to see all SWIMS user accounts. The list shows each user's name, email, account type (Local or AD), active status, and assigned roles.

## Creating a New User

1. Click **New User**.
2. Fill in the details:
   - **Username / Email** — the user's email address (used for login and notifications)
   - **Display Name** — how their name appears in the system
   - **Temporary Password** — they will be prompted to change this on first login
3. Assign one or more **Roles** (see [Roles](roles.md)).
4. Click **Create**.

SWIMS will send a confirmation email to the new user if the email service is configured.

> [!NOTE]
> For Active Directory (network) accounts, you do not need to create the user manually. They are automatically created in SWIMS on their **first successful login** using their AD credentials. Roles must still be assigned by an admin afterwards.

## Editing a User

Click a user's name to open their profile. From here you can:

| Action | Description |
|--------|-------------|
| **Edit Profile** | Update display name or email |
| **Assign / Remove Roles** | Add or remove roles; changes take effect on the user's next login |
| **Lock Account** | Prevent the user from logging in (e.g. while on leave, or for security) |
| **Unlock Account** | Re-enable a locked account |
| **Send Password Reset** | Trigger a password reset email (local accounts only) |
| **Reset 2FA** | Clear the user's two-factor authentication setup (use if they are locked out) |

## Locking an Account

Locking prevents the user from logging in without deleting their data or history. Use this for temporary suspension.

1. Open the user profile.
2. Click **Lock Account**.
3. Confirm. The user is immediately locked out.

To unlock, return to the profile and click **Unlock Account**.

## Role Assignment

Role changes take effect when the user **next logs in**. If you need the change to apply immediately, ask the user to log out and log back in.

## LDAP (Active Directory) Users

AD users show **AD Account** in their account type column. For these users:

- Password change, email change, and 2FA are managed by IT/Active Directory — not SWIMS
- Role assignments are managed within SWIMS as normal
- AD group memberships are synced to SWIMS roles automatically on each login
