using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AccessControl_PublicAndAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "endpoint_policy_assignments",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Controller = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Page = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Regex = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    PolicyName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_endpoint_policy_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_endpoint_policy_assignments_policies_PolicyId",
                        column: x => x.PolicyId,
                        principalSchema: "auth",
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "public_endpoints",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Controller = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Page = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Regex = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_endpoints", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_endpoint_policy_assignments_MatchType_Area_Controller_Action_Page_Path_Regex_PolicyId_IsEnabled",
                schema: "auth",
                table: "endpoint_policy_assignments",
                columns: new[] { "MatchType", "Area", "Controller", "Action", "Page", "Path", "Regex", "PolicyId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_endpoint_policy_assignments_PolicyId",
                schema: "auth",
                table: "endpoint_policy_assignments",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_public_endpoints_MatchType_Area_Controller_Action_Page_Path_Regex_IsEnabled",
                schema: "auth",
                table: "public_endpoints",
                columns: new[] { "MatchType", "Area", "Controller", "Action", "Page", "Path", "Regex", "IsEnabled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "endpoint_policy_assignments",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "public_endpoints",
                schema: "auth");
        }
    }
}
