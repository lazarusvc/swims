using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Cases
{
    /// <inheritdoc />
    public partial class Cases_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "case");

            migrationBuilder.CreateTable(
                name: "SW_case",
                schema: "case",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    case_number = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SW_beneficiaryId = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    program_tag = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    closed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SW_case", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SW_caseAssignment",
                schema: "case",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SW_caseId = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    role_on_case = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    unassigned_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SW_caseAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SW_caseAssignment_SW_case_SW_caseId",
                        column: x => x.SW_caseId,
                        principalSchema: "case",
                        principalTable: "SW_case",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SW_caseForm",
                schema: "case",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SW_caseId = table.Column<int>(type: "int", nullable: false),
                    SW_formTableDatumId = table.Column<int>(type: "int", nullable: false),
                    form_role = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    is_primary_application = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    linked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    linked_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SW_caseForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SW_caseForm_SW_case_SW_caseId",
                        column: x => x.SW_caseId,
                        principalSchema: "case",
                        principalTable: "SW_case",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SW_caseAssignment_SW_caseId",
                schema: "case",
                table: "SW_caseAssignment",
                column: "SW_caseId");

            migrationBuilder.CreateIndex(
                name: "IX_SW_caseForm_SW_caseId",
                schema: "case",
                table: "SW_caseForm",
                column: "SW_caseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SW_caseAssignment",
                schema: "case");

            migrationBuilder.DropTable(
                name: "SW_caseForm",
                schema: "case");

            migrationBuilder.DropTable(
                name: "SW_case",
                schema: "case");
        }
    }
}
