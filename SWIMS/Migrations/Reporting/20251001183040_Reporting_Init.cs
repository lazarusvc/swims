using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Reporting
{
    /// <inheritdoc />
    public partial class Reporting_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rpt");

            migrationBuilder.CreateTable(
                name: "SW_reports",
                schema: "rpt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    PathOverride = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParamCheck = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SW_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SW_reports_params",
                schema: "rpt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParamKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ParamValue = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ParamDataType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    SwReportId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SW_reports_params", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SW_reports_params_SW_reports_SwReportId",
                        column: x => x.SwReportId,
                        principalSchema: "rpt",
                        principalTable: "SW_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SW_reports_params_SwReportId",
                schema: "rpt",
                table: "SW_reports_params",
                column: "SwReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SW_reports_params",
                schema: "rpt");

            migrationBuilder.DropTable(
                name: "SW_reports",
                schema: "rpt");
        }
    }
}
