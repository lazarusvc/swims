using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.Reports
{
    /// <inheritdoc />
    public partial class Reports_Init : Migration
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
                name: "SwSiteIdentity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Media01 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Media02 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Media03 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwSiteIdentity", x => x.Id);
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
                    SwReportId = table.Column<int>(type: "int", nullable: false),
                    SwSiteIdentityId = table.Column<int>(type: "int", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_SW_reports_params_SwSiteIdentity_SwSiteIdentityId",
                        column: x => x.SwSiteIdentityId,
                        principalTable: "SwSiteIdentity",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SwForm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Form = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsApproval01 = table.Column<byte>(type: "tinyint", nullable: true),
                    IsApproval02 = table.Column<byte>(type: "tinyint", nullable: true),
                    IsApproval03 = table.Column<byte>(type: "tinyint", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SwIdentityId = table.Column<int>(type: "int", nullable: false),
                    FormData01 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData02 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData03 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData04 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData05 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData06 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData07 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData08 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData09 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData10 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData11 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData12 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData13 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData14 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData15 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData16 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData17 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData18 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData19 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData20 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData21 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData22 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData23 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData24 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData25 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData26 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData27 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData28 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData29 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData30 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData31 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData32 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData33 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData34 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData35 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData36 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData37 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData38 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData39 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData40 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData41 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData42 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData43 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData44 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData45 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData46 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData47 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData48 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData49 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData50 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData51 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData52 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData53 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData54 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData55 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData56 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData57 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData58 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData59 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData60 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData61 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData62 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData63 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData65 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData66 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData67 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData68 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData69 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData70 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData71 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData72 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData73 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData74 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData75 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData76 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData77 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData78 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData79 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData80 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData81 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData82 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData83 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData84 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData85 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData86 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData87 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData88 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData89 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData90 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData91 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData92 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData93 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData94 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData95 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData96 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData97 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData98 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData99 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData100 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData101 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData102 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData103 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData104 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData105 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData106 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData107 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData108 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData109 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData110 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData111 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData112 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData113 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData114 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData115 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData116 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData117 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData118 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData119 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData120 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData121 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData122 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData123 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData124 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData125 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData126 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData127 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData128 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData129 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData130 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData131 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData132 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData133 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData134 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData135 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData136 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData137 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData138 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData139 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData140 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData141 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData142 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData143 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData144 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData145 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData146 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData147 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData148 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData149 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData150 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData151 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData152 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData153 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData154 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData155 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData156 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData157 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData158 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData159 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData160 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData161 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData162 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData163 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData164 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData165 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData166 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData167 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData168 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData169 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData170 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData171 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData172 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData173 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData174 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData175 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData176 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData177 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData178 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData179 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData180 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData181 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData182 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData183 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData184 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData185 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData186 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData187 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData188 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData189 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData190 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData191 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData192 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData193 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData194 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData195 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData196 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData197 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData198 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData199 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData200 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData201 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData202 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData203 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData204 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData205 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData206 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData207 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData208 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData209 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData210 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData211 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData212 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData213 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData214 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData215 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData216 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData217 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData218 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData219 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData220 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData221 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData222 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData223 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData224 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData225 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData226 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData227 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData228 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData229 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData230 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData231 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData232 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData233 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData234 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData235 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData236 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData237 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData238 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData239 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData240 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData241 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData242 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData243 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData244 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData245 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData246 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData247 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData248 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData249 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormData250 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwForm_SwSiteIdentity_SwIdentityId",
                        column: x => x.SwIdentityId,
                        principalTable: "SwSiteIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SwFormsTableName",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SwFormsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwFormsTableName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwFormsTableName_SwForm_SwFormsId",
                        column: x => x.SwFormsId,
                        principalTable: "SwForm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SW_reports_params_SwReportId",
                schema: "rpt",
                table: "SW_reports_params",
                column: "SwReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SW_reports_params_SwSiteIdentityId",
                schema: "rpt",
                table: "SW_reports_params",
                column: "SwSiteIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_SwForm_SwIdentityId",
                table: "SwForm",
                column: "SwIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_SwFormsTableName_SwFormsId",
                table: "SwFormsTableName",
                column: "SwFormsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SW_reports_params",
                schema: "rpt");

            migrationBuilder.DropTable(
                name: "SwFormsTableName");

            migrationBuilder.DropTable(
                name: "SW_reports",
                schema: "rpt");

            migrationBuilder.DropTable(
                name: "SwForm");

            migrationBuilder.DropTable(
                name: "SwSiteIdentity");
        }
    }
}
