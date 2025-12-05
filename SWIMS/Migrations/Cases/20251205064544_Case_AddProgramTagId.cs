using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Cases
{
    /// <inheritdoc />
    public partial class Case_AddProgramTagId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add the nullable FK column on case.SW_case
            migrationBuilder.AddColumn<int>(
                name: "ProgramTagId",
                schema: "case",
                table: "SW_case",
                type: "int",
                nullable: true);

            // 2) Index to help future filtering by program tag
            migrationBuilder.CreateIndex(
                name: "IX_SW_case_ProgramTagId",
                schema: "case",
                table: "SW_case",
                column: "ProgramTagId");

            // 3) Hard FK to ref.SW_programTag, but allow NULL values
            migrationBuilder.Sql(@"
        ALTER TABLE [case].[SW_case] WITH CHECK
        ADD CONSTRAINT [FK_SW_case_SW_programTag_ProgramTagId]
        FOREIGN KEY ([ProgramTagId]) REFERENCES [ref].[SW_programTag]([Id]);
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK first
            migrationBuilder.Sql(@"
        ALTER TABLE [case].[SW_case]
        DROP CONSTRAINT [FK_SW_case_SW_programTag_ProgramTagId];
    ");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_SW_case_ProgramTagId",
                schema: "case",
                table: "SW_case");

            // Drop column
            migrationBuilder.DropColumn(
                name: "ProgramTagId",
                schema: "case",
                table: "SW_case");
        }

    }
}
