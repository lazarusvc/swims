using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.StoredProcs
{
    /// <inheritdoc />
    public partial class StoredProcs_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sp");

            migrationBuilder.CreateTable(
                name: "stored_processes",
                schema: "sp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ConnectionKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DataSource = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Database = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DbUserEncrypted = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DbPasswordEncrypted = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stored_processes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stored_process_params",
                schema: "sp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoredProcessId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stored_process_params", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stored_process_params_stored_processes_StoredProcessId",
                        column: x => x.StoredProcessId,
                        principalSchema: "sp",
                        principalTable: "stored_processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stored_process_params_StoredProcessId",
                schema: "sp",
                table: "stored_process_params",
                column: "StoredProcessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stored_process_params",
                schema: "sp");

            migrationBuilder.DropTable(
                name: "stored_processes",
                schema: "sp");
        }
    }
}
