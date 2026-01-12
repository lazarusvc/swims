using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Cases
{
    /// <inheritdoc />
    public partial class Cases_AddBenefitPeriodFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "benefit_end_at",
                schema: "case",
                table: "SW_case",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "benefit_end_at_override",
                schema: "case",
                table: "SW_case",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "benefit_period_months",
                schema: "case",
                table: "SW_case",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "benefit_period_months_override",
                schema: "case",
                table: "SW_case",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "benefit_period_source",
                schema: "case",
                table: "SW_case",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "benefit_start_at",
                schema: "case",
                table: "SW_case",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "benefit_start_at_override",
                schema: "case",
                table: "SW_case",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "benefit_end_at",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "benefit_end_at_override",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "benefit_period_months",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "benefit_period_months_override",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "benefit_period_source",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "benefit_start_at",
                schema: "case",
                table: "SW_case");

            migrationBuilder.DropColumn(
                name: "benefit_start_at_override",
                schema: "case",
                table: "SW_case");
        }
    }
}
