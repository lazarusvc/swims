/// Migrations commands


# Before any PMC migration/update, run this quick check in SSMS:

## SELECT DB_NAME() AS CurrentDb;


# Adding Migrations

## Add-Migration "SwimsIdentity_Init" -Context SwimsIdentityDbContext -Output Migrations/Identity
## Add-Migration "StoredProcs_Init" -Context SwimsStoredProcsDbContext -Output Migrations/StoredProcs
## Add-Migration "Reporting_Init" -Context SWIMS.Data.Reports.SwimsReportsDbContext -Output Migrations/Reporting
## Add-Migration "DbMore_Init" -Context SwimsDb_moreContext -Output Migrations/DbMore
## Add-Migration Cases_Init -Context SwimsCasesDbContext -OutputDir "Migrations/Cases"
## Add-Migration Lookup_Init -Context SwimsLookupDbContext -OutputDir "Migrations/Lookup"


# Updating Database

## Update-Database -Context SwimsIdentityDbContext
## Update-Database -Context SwimsStoredProcsDbContext
## Update-Database -Context SWIMS.Data.Reports.SwimsReportsDbContext
## Update-Database -Context SwimsDb_moreContext
## Update-Database -Context SwimsCasesDbContext
## Update-Database -Context SwimsLookupDbContext


# Good to Know

## Get-Migration -Context SwimsIdentityDbContext
## Update-Database <PreviousMigrationId> -Context SwimsIdentityDbContext   # rollback if needed
## Remove-Migration -Context SwimsIdentityDbContext                        # removes last migration + updates snapshot


