using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Cases
{
    /// <inheritdoc />
    public partial class AddCaseStatusOverrideColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status_override",
                schema: "case",
                table: "SW_case",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "status_override_at",
                schema: "case",
                table: "SW_case",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_override_by",
                schema: "case",
                table: "SW_case",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_override_reason",
                schema: "case",
                table: "SW_case",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "status_override_until",
                schema: "case",
                table: "SW_case",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status_override",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "status_override_at",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "status_override_by",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "status_override_reason",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "status_override_until",
                schema: "case",
                table: "SW_case");
        }
    }
}
