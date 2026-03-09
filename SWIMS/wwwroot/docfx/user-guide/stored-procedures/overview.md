# Stored Procedures — Overview

*This section is for users with the `SP.Run` or `SP.Manage` permission — typically senior administrators and technical staff.*

The **Stored Procedures** module allows authorised staff to run pre-configured SQL database procedures through a controlled web interface, without needing direct database access.

## What Are Stored Procedures Used For?

Common use cases include:

- Generating data extracts (e.g. lists of active beneficiaries by programme)
- Running scheduled maintenance tasks
- Preparing payment lists for SmartStream or Welfare Form 6/7 export
- Triggering data synchronisation with external systems

## Running a Stored Procedure

1. Navigate to **Stored Procedures** in the sidebar (you must have `SP.Run` permission).
2. The list shows all available procedures with their names and descriptions.
3. Click **Run** next to the procedure you want to execute.
4. Fill in any required parameters in the form shown.
5. Click **Execute**.
6. Results are displayed in a table on screen.

## Parameters

Each procedure may ask for different parameters. Common ones:

| Parameter | Description |
|-----------|-------------|
| **Date From / Date To** | Date range for the operation |
| **Programme** | Which programme to process |
| **Max Rows** | Limit the number of rows returned |
| **Format** | Output format (table, CSV summary) |

Required parameters are marked with `*`. The procedure cannot run until all required parameters are filled in.

## Exporting Results

Results displayed on screen can typically be exported to CSV using the **Export** button that appears after execution.

## Managing Procedures (Admin)

Users with `SP.Manage` permission can:

- Register new stored procedures in the catalogue (**Stored Procedure Admin**)
- Edit procedure names, descriptions, and connection settings
- Configure and manage parameters (**Stored Procedure Params**)

> [!WARNING]
> Stored procedures execute directly against the SWIMS database. Only register and run procedures that have been reviewed and approved by your database administrator.
