/// Migrations commands

# Adding Migrations

## Add-Migration "SwimsIdentity_Init" -Context SwimsIdentityDbContext -Output Migrations/Identity
## Add-Migration "StoredProcs_Init" -Context SwimsStoredProcsDbContext -Output Migrations/StoredProcs
## Add-Migration "Reporting_Init" -Context SWIMS.Data.Reports.SwimsReportsDbContext -Output Migrations/Reporting
## Add-Migration "DbMore_Init" -Context SwimsDb_moreContext -Output Migrations/DbMore

# Updating Database

## Update-Database -Context SwimsIdentityDbContext
## Update-Database -Context SwimsStoredProcsDbContext
## Update-Database -Context SWIMS.Data.Reports.SwimsReportsDbContext
## Update-Database -Context SwimsDb_moreContext
