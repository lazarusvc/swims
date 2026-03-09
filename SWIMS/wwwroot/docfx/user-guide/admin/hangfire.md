# Hangfire Dashboard

*Requires `Admin.Hangfire` permission.*

Navigate to **Admin → Hangfire** (or directly to `/hangfire`).

The Hangfire Dashboard gives you visibility into the background jobs that SWIMS runs automatically. These jobs handle email sending, notification delivery, daily digests, and case maintenance.

## What You'll See

### Summary Cards (top of page)

| Card | Description |
|------|-------------|
| **Enqueued** | Jobs waiting to be picked up by a worker |
| **Processing** | Jobs currently running |
| **Scheduled** | Jobs scheduled to run at a future time |
| **Succeeded** | Jobs that completed successfully |
| **Failed** | Jobs that failed after retries |
| **Deleted** | Jobs manually deleted |

### Recurring Jobs

Click **Recurring Jobs** to see the scheduled recurring tasks:

| Job | Schedule | What it does |
|-----|---------|-------------|
| `email-outbox-dispatch` | Every minute | Sends queued emails from the outbox |
| `notification-digest-daily` | 8:00 AM daily | Sends the daily notification digest email to all users |

### Servers

Shows the Hangfire server instances and which queues they are processing.

## Common Actions

### Retrying a Failed Job

If a job has failed:

1. Click **Failed** in the left menu.
2. Find the failed job and click it to see the error details.
3. Click **Retry** to attempt it again.

### Deleting a Failed Job

If a job is failing repeatedly and you want to clear it:

1. Click **Failed**.
2. Find the job.
3. Click **Delete**.

> [!WARNING]
> Deleting a failed job means it will not be retried. If it was a notification or email job, that notification or email will not be sent. Only delete jobs you are certain are no longer needed.

## When to Check Hangfire

Check the Hangfire dashboard if:

- Users report they are not receiving email notifications
- The daily digest emails are not arriving
- Background maintenance seems to not be running (e.g. cases that should be Inactive are still Active)
