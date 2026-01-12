using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Lookup
{
    /// <inheritdoc />
    public partial class lookup_programtag_default_benefit_months : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "default_benefit_months",
                schema: "ref",
                table: "SW_programTag",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "default_benefit_months",
                schema: "ref",
                table: "SW_programTag");
        }
    }
}
