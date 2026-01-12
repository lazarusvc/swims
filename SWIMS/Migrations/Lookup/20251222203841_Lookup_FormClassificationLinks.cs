using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Lookup
{
    /// <inheritdoc />
    public partial class Lookup_FormClassificationLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SW_formFormType",
                schema: "ref",
                columns: table => new
                {
                    SW_formsId = table.Column<int>(type: "int", nullable: false),
                    SW_formTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SW_formFormType", x => x.SW_formsId);
                    table.ForeignKey(
                        name: "FK_SW_formFormType_SW_formType_SW_formTypeId",
                        column: x => x.SW_formTypeId,
                        principalSchema: "ref",
                        principalTable: "SW_formType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SW_formProgramTag",
                schema: "ref",
                columns: table => new
                {
                    SW_formsId = table.Column<int>(type: "int", nullable: false),
                    SW_programTagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SW_formProgramTag", x => new { x.SW_formsId, x.SW_programTagId });
                    table.ForeignKey(
                        name: "FK_SW_formProgramTag_SW_programTag_SW_programTagId",
                        column: x => x.SW_programTagId,
                        principalSchema: "ref",
                        principalTable: "SW_programTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SW_formFormType_SW_formTypeId",
                schema: "ref",
                table: "SW_formFormType",
                column: "SW_formTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SW_formProgramTag_SW_formsId",
                schema: "ref",
                table: "SW_formProgramTag",
                column: "SW_formsId");

            migrationBuilder.CreateIndex(
                name: "IX_SW_formProgramTag_SW_programTagId",
                schema: "ref",
                table: "SW_formProgramTag",
                column: "SW_programTagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SW_formFormType",
                schema: "ref");

            migrationBuilder.DropTable(
                name: "SW_formProgramTag",
                schema: "ref");
        }
    }
}
