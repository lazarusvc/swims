using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.StoredProcs
{
    /// <inheritdoc />
    public partial class AddExcludeHeadersOnExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: safe to run even if the column already exists or
            // the migration history is out of sync (e.g. on Austin's DB).
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM   sys.columns
                    WHERE  object_id = OBJECT_ID(N'sp.stored_processes')
                      AND  name = N'ExcludeHeadersOnExport'
                )
                BEGIN
                    ALTER TABLE [sp].[stored_processes]
                        ADD [ExcludeHeadersOnExport] bit NOT NULL
                            CONSTRAINT [DF_stored_processes_ExcludeHeadersOnExport] DEFAULT 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExcludeHeadersOnExport",
                schema: "sp",
                table: "stored_processes");
        }
    }
}
