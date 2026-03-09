# Common Tasks — Cheat Sheet

Quick steps for the most frequent actions in SWIMS.

---

## Register a New Beneficiary
1. **Beneficiaries** → search first to avoid duplicates
2. **Register New Beneficiary** → fill in personal details, ID, contact info → **Save**

---

## Create a Case
1. **Cases** → **New Case**
2. Select beneficiary and programme → **Save**
3. From the Case Detail → **Link Form** → select Primary Application submission → **Save**
4. Case activates automatically once the linked form is fully approved

---

## Submit an Application Form
1. **Programmes** → find the relevant programme and form
2. **Fill In** → complete all required fields (`*`)
3. **Submit** → form enters the Level 1 approval queue

---

## Approve a Submission
1. **Approvals** → find the form with pending submissions
2. Click a submission → review all fields
3. **Approve** (or **Reject** with notes) → **Confirm**

---

## Find a Beneficiary
1. **Beneficiaries** → type name or ID number in the search bar (3+ characters)
2. Click the result to open their profile

---

## Find a Case
- By number: search bar → type `CASE-YYYY-NNNNN`
- By beneficiary: open the beneficiary profile → **Cases** tab
- By filter: **Cases** → use Status / Programme / Officer filters

---

## Change a Case Status (Override)
1. **Cases** → open the case
2. **Edit** → Status Override → select new status → set expiry date if temporary → **Save**

---

## Send a Message to a Colleague
1. **Messenger** → **New Conversation**
2. Search for the colleague → **Start Conversation**
3. Type message → **Enter** or **Send**

---

## Run a Report
1. **Reports** → click the report name
2. Fill in parameters (date range, programme, etc.)
3. **Run Report** → view inline or **Download PDF**

---

## Set Up Two-Factor Authentication
1. **Account** (top-right) → **Two-Factor Authentication** → **Set Up Authenticator App**
2. Scan QR code with your authenticator app
3. Enter 6-digit code → **Verify and Enable**
4. Save recovery codes securely

---

## Add a New User (Admin)
1. **Admin → Users** → **New User**
2. Enter email, display name, temporary password → assign roles → **Create**

---

## Check Why Emails Are Not Sending (Admin)
1. **Admin → Hangfire** → click **Failed**
2. Check for failed `email-outbox-dispatch` jobs
3. Click the job to see the error → **Retry**

---

## Lock a User Account (Admin)
1. **Admin → Users** → find the user
2. Click their name → **Lock Account** → **Confirm**
