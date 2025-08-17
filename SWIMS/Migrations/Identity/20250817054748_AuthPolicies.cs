using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AuthPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policies",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "policy_claims",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorizationPolicyEntityId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_policy_claims_policies_AuthorizationPolicyEntityId",
                        column: x => x.AuthorizationPolicyEntityId,
                        principalSchema: "auth",
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "policy_roles",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorizationPolicyEntityId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_policy_roles_policies_AuthorizationPolicyEntityId",
                        column: x => x.AuthorizationPolicyEntityId,
                        principalSchema: "auth",
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_policy_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_policies_Name",
                schema: "auth",
                table: "policies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_claims_AuthorizationPolicyEntityId",
                schema: "auth",
                table: "policy_claims",
                column: "AuthorizationPolicyEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_policy_roles_AuthorizationPolicyEntityId_RoleId",
                schema: "auth",
                table: "policy_roles",
                columns: new[] { "AuthorizationPolicyEntityId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_roles_RoleId",
                schema: "auth",
                table: "policy_roles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_claims",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "policy_roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "policies",
                schema: "auth");
        }
    }
}
