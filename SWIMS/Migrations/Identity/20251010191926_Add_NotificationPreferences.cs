using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Identity
{
    /// <inheritdoc />
    public partial class Add_NotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_prefs",
                schema: "notify",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    InAppEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DigestEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_prefs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_prefs_UserId_Type",
                schema: "notify",
                table: "notification_prefs",
                columns: new[] { "UserId", "Type" },
                unique: true,
                filter: "[Type] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_prefs",
                schema: "notify");
        }
    }
}
