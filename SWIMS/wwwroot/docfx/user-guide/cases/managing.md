# Managing a Case

## Finding a Case

**All Cases**: Navigate to **Cases** in the sidebar to see the full case list. Use the filter controls to narrow by status, programme, assigned officer, or date range.

**My Cases**: Click **My Cases** to see only cases assigned to you.

**By Case Number**: Use the search bar and enter the case number (e.g. `CASE-2025-00042`).

## The Case Detail View

Clicking a case opens its Detail view, which shows:

- **Case header**: case number, beneficiary name, programme, status
- **Benefit Period**: start date, end date, duration in months, and the source of the period (computed or manually overridden)
- **Linked Forms**: all form submissions linked to this case, with their approval status
- **Case Worker Assignment**: who the case is currently assigned to
- **Status**: current lifecycle status with any active override noted

## Linking Form Submissions

You can link one or more submitted forms to a case:

1. From the Case Detail page, click **Link Form**.
2. Search for the form submission by form name, beneficiary, or submission date.
3. Select the submission from the results.
4. Optionally check **Mark as Primary Application** — this designates the submission as the one that drives the case's status and benefit period. Only one Primary Application can exist per case.
5. Click **Link**.

> [!IMPORTANT]
> Only one form submission can be the **Primary Application** at a time. Setting a new Primary Application automatically removes the previous one's primary designation. The case status will be immediately recalculated based on the new Primary Application.

## Unlinking a Form

To remove a form submission from a case, open the Case Detail and click **Unlink** next to the submission. This does not delete the submission — it only removes the association.

## Manually Refreshing Case Status

If a case status seems out of date (e.g. an approval was just completed but the case still shows Pending), click **Refresh Status** on the Case Detail page. This triggers an immediate recalculation from the Primary Application's current approval state.

## Overriding the Case Status

In exceptional circumstances, you can manually override the computed status:

1. From the Case Detail page, click **Edit** or find the status override section.
2. Select the override status (Active, Inactive, Closed, or Pending).
3. Optionally set an **Override Until** date — after this date, the override expires automatically and the computed status takes over again.
4. Save.

> [!WARNING]
> A manual status override **always wins** over the computed status for as long as it is active. Use overrides carefully and document the reason in any associated case notes or communications.

## Managing the Benefit Period

To view or override the benefit period:

1. From the Case Detail page, click **Benefit Period**.
2. The current effective start date, end date, and duration are shown, along with whether they came from the system calculation or a previous override.
3. To override:
   - Enter a new **Start Date**, **End Date**, or **Duration in Months**.
   - Click **Save Override**.
4. To remove an override and revert to the computed values:
   - Click **Clear Override**.

After saving or clearing, the case status is immediately recalculated.

## Reassigning a Case

1. From the Case Detail page, click **Assign**.
2. Search for and select the new case worker.
3. Click **Save Assignment**.

## Case Status Automation

SWIMS automatically monitors active cases in the background:

- When a case's benefit end date passes and there is no active status override, the system will automatically set the status to **Inactive**.
- Timed status overrides are automatically cleared when their expiry date passes.

No action is needed from you — this happens automatically overnight.
