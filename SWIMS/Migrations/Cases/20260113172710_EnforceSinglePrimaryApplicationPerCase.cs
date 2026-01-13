using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Cases
{
    /// <inheritdoc />
    public partial class EnforceSinglePrimaryApplicationPerCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ;WITH Ranked AS (
                SELECT Id,
                       SW_caseId,
                       ROW_NUMBER() OVER (PARTITION BY SW_caseId ORDER BY linked_at DESC, Id DESC) AS rn
                FROM [case].[SW_caseForm]
                WHERE is_primary_application = 1
            )
            UPDATE [case].[SW_caseForm]
            SET is_primary_application = 0
            WHERE Id IN (SELECT Id FROM Ranked WHERE rn > 1);
            ");

            migrationBuilder.Sql(@"
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'UX_SW_caseForm_PrimaryApplication_PerCase'
                    AND object_id = OBJECT_ID('[case].[SW_caseForm]')
            )
            BEGIN
                CREATE UNIQUE INDEX UX_SW_caseForm_PrimaryApplication_PerCase
                ON [case].[SW_caseForm](SW_caseId)
                WHERE is_primary_application = 1;
            END
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'UX_SW_caseForm_PrimaryApplication_PerCase'
                  AND object_id = OBJECT_ID('[case].[SW_caseForm]')
            )
            BEGIN
                DROP INDEX UX_SW_caseForm_PrimaryApplication_PerCase ON [case].[SW_caseForm];
            END
            ");

        }
    }
}
