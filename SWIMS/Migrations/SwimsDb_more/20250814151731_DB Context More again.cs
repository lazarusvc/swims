using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWIMS.Migrations.SwimsDb_more
{
    /// <inheritdoc />
    public partial class DBContextMoreagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SW_forms_tableNames");

            migrationBuilder.RenameColumn(
                name: "is_approval_03",
                table: "SW_forms",
                newName: "isApproval_03");

            migrationBuilder.RenameColumn(
                name: "is_approval_02",
                table: "SW_forms",
                newName: "isApproval_02");

            migrationBuilder.RenameColumn(
                name: "is_approval_01",
                table: "SW_forms",
                newName: "isApproval_01");

            migrationBuilder.RenameColumn(
                name: "formData_99",
                table: "SW_forms",
                newName: "FormData99");

            migrationBuilder.RenameColumn(
                name: "formData_98",
                table: "SW_forms",
                newName: "FormData98");

            migrationBuilder.RenameColumn(
                name: "formData_97",
                table: "SW_forms",
                newName: "FormData97");

            migrationBuilder.RenameColumn(
                name: "formData_96",
                table: "SW_forms",
                newName: "FormData96");

            migrationBuilder.RenameColumn(
                name: "formData_95",
                table: "SW_forms",
                newName: "FormData95");

            migrationBuilder.RenameColumn(
                name: "formData_94",
                table: "SW_forms",
                newName: "FormData94");

            migrationBuilder.RenameColumn(
                name: "formData_93",
                table: "SW_forms",
                newName: "FormData93");

            migrationBuilder.RenameColumn(
                name: "formData_92",
                table: "SW_forms",
                newName: "FormData92");

            migrationBuilder.RenameColumn(
                name: "formData_91",
                table: "SW_forms",
                newName: "FormData91");

            migrationBuilder.RenameColumn(
                name: "formData_90",
                table: "SW_forms",
                newName: "FormData90");

            migrationBuilder.RenameColumn(
                name: "formData_89",
                table: "SW_forms",
                newName: "FormData89");

            migrationBuilder.RenameColumn(
                name: "formData_88",
                table: "SW_forms",
                newName: "FormData88");

            migrationBuilder.RenameColumn(
                name: "formData_87",
                table: "SW_forms",
                newName: "FormData87");

            migrationBuilder.RenameColumn(
                name: "formData_86",
                table: "SW_forms",
                newName: "FormData86");

            migrationBuilder.RenameColumn(
                name: "formData_85",
                table: "SW_forms",
                newName: "FormData85");

            migrationBuilder.RenameColumn(
                name: "formData_84",
                table: "SW_forms",
                newName: "FormData84");

            migrationBuilder.RenameColumn(
                name: "formData_83",
                table: "SW_forms",
                newName: "FormData83");

            migrationBuilder.RenameColumn(
                name: "formData_82",
                table: "SW_forms",
                newName: "FormData82");

            migrationBuilder.RenameColumn(
                name: "formData_81",
                table: "SW_forms",
                newName: "FormData81");

            migrationBuilder.RenameColumn(
                name: "formData_80",
                table: "SW_forms",
                newName: "FormData80");

            migrationBuilder.RenameColumn(
                name: "formData_79",
                table: "SW_forms",
                newName: "FormData79");

            migrationBuilder.RenameColumn(
                name: "formData_78",
                table: "SW_forms",
                newName: "FormData78");

            migrationBuilder.RenameColumn(
                name: "formData_77",
                table: "SW_forms",
                newName: "FormData77");

            migrationBuilder.RenameColumn(
                name: "formData_76",
                table: "SW_forms",
                newName: "FormData76");

            migrationBuilder.RenameColumn(
                name: "formData_75",
                table: "SW_forms",
                newName: "FormData75");

            migrationBuilder.RenameColumn(
                name: "formData_74",
                table: "SW_forms",
                newName: "FormData74");

            migrationBuilder.RenameColumn(
                name: "formData_73",
                table: "SW_forms",
                newName: "FormData73");

            migrationBuilder.RenameColumn(
                name: "formData_72",
                table: "SW_forms",
                newName: "FormData72");

            migrationBuilder.RenameColumn(
                name: "formData_71",
                table: "SW_forms",
                newName: "FormData71");

            migrationBuilder.RenameColumn(
                name: "formData_70",
                table: "SW_forms",
                newName: "FormData70");

            migrationBuilder.RenameColumn(
                name: "formData_69",
                table: "SW_forms",
                newName: "FormData69");

            migrationBuilder.RenameColumn(
                name: "formData_68",
                table: "SW_forms",
                newName: "FormData68");

            migrationBuilder.RenameColumn(
                name: "formData_67",
                table: "SW_forms",
                newName: "FormData67");

            migrationBuilder.RenameColumn(
                name: "formData_66",
                table: "SW_forms",
                newName: "FormData66");

            migrationBuilder.RenameColumn(
                name: "formData_65",
                table: "SW_forms",
                newName: "FormData65");

            migrationBuilder.RenameColumn(
                name: "formData_64",
                table: "SW_forms",
                newName: "FormData64");

            migrationBuilder.RenameColumn(
                name: "formData_63",
                table: "SW_forms",
                newName: "FormData63");

            migrationBuilder.RenameColumn(
                name: "formData_62",
                table: "SW_forms",
                newName: "FormData62");

            migrationBuilder.RenameColumn(
                name: "formData_61",
                table: "SW_forms",
                newName: "FormData61");

            migrationBuilder.RenameColumn(
                name: "formData_60",
                table: "SW_forms",
                newName: "FormData60");

            migrationBuilder.RenameColumn(
                name: "formData_59",
                table: "SW_forms",
                newName: "FormData59");

            migrationBuilder.RenameColumn(
                name: "formData_58",
                table: "SW_forms",
                newName: "FormData58");

            migrationBuilder.RenameColumn(
                name: "formData_57",
                table: "SW_forms",
                newName: "FormData57");

            migrationBuilder.RenameColumn(
                name: "formData_56",
                table: "SW_forms",
                newName: "FormData56");

            migrationBuilder.RenameColumn(
                name: "formData_55",
                table: "SW_forms",
                newName: "FormData55");

            migrationBuilder.RenameColumn(
                name: "formData_54",
                table: "SW_forms",
                newName: "FormData54");

            migrationBuilder.RenameColumn(
                name: "formData_53",
                table: "SW_forms",
                newName: "FormData53");

            migrationBuilder.RenameColumn(
                name: "formData_52",
                table: "SW_forms",
                newName: "FormData52");

            migrationBuilder.RenameColumn(
                name: "formData_51",
                table: "SW_forms",
                newName: "FormData51");

            migrationBuilder.RenameColumn(
                name: "formData_50",
                table: "SW_forms",
                newName: "FormData50");

            migrationBuilder.RenameColumn(
                name: "formData_49",
                table: "SW_forms",
                newName: "FormData49");

            migrationBuilder.RenameColumn(
                name: "formData_48",
                table: "SW_forms",
                newName: "FormData48");

            migrationBuilder.RenameColumn(
                name: "formData_47",
                table: "SW_forms",
                newName: "FormData47");

            migrationBuilder.RenameColumn(
                name: "formData_46",
                table: "SW_forms",
                newName: "FormData46");

            migrationBuilder.RenameColumn(
                name: "formData_45",
                table: "SW_forms",
                newName: "FormData45");

            migrationBuilder.RenameColumn(
                name: "formData_44",
                table: "SW_forms",
                newName: "FormData44");

            migrationBuilder.RenameColumn(
                name: "formData_43",
                table: "SW_forms",
                newName: "FormData43");

            migrationBuilder.RenameColumn(
                name: "formData_42",
                table: "SW_forms",
                newName: "FormData42");

            migrationBuilder.RenameColumn(
                name: "formData_41",
                table: "SW_forms",
                newName: "FormData41");

            migrationBuilder.RenameColumn(
                name: "formData_40",
                table: "SW_forms",
                newName: "FormData40");

            migrationBuilder.RenameColumn(
                name: "formData_39",
                table: "SW_forms",
                newName: "FormData39");

            migrationBuilder.RenameColumn(
                name: "formData_38",
                table: "SW_forms",
                newName: "FormData38");

            migrationBuilder.RenameColumn(
                name: "formData_37",
                table: "SW_forms",
                newName: "FormData37");

            migrationBuilder.RenameColumn(
                name: "formData_36",
                table: "SW_forms",
                newName: "FormData36");

            migrationBuilder.RenameColumn(
                name: "formData_35",
                table: "SW_forms",
                newName: "FormData35");

            migrationBuilder.RenameColumn(
                name: "formData_34",
                table: "SW_forms",
                newName: "FormData34");

            migrationBuilder.RenameColumn(
                name: "formData_33",
                table: "SW_forms",
                newName: "FormData33");

            migrationBuilder.RenameColumn(
                name: "formData_32",
                table: "SW_forms",
                newName: "FormData32");

            migrationBuilder.RenameColumn(
                name: "formData_31",
                table: "SW_forms",
                newName: "FormData31");

            migrationBuilder.RenameColumn(
                name: "formData_30",
                table: "SW_forms",
                newName: "FormData30");

            migrationBuilder.RenameColumn(
                name: "formData_29",
                table: "SW_forms",
                newName: "FormData29");

            migrationBuilder.RenameColumn(
                name: "formData_28",
                table: "SW_forms",
                newName: "FormData28");

            migrationBuilder.RenameColumn(
                name: "formData_27",
                table: "SW_forms",
                newName: "FormData27");

            migrationBuilder.RenameColumn(
                name: "formData_26",
                table: "SW_forms",
                newName: "FormData26");

            migrationBuilder.RenameColumn(
                name: "formData_250",
                table: "SW_forms",
                newName: "FormData250");

            migrationBuilder.RenameColumn(
                name: "formData_25",
                table: "SW_forms",
                newName: "FormData25");

            migrationBuilder.RenameColumn(
                name: "formData_249",
                table: "SW_forms",
                newName: "FormData249");

            migrationBuilder.RenameColumn(
                name: "formData_248",
                table: "SW_forms",
                newName: "FormData248");

            migrationBuilder.RenameColumn(
                name: "formData_247",
                table: "SW_forms",
                newName: "FormData247");

            migrationBuilder.RenameColumn(
                name: "formData_246",
                table: "SW_forms",
                newName: "FormData246");

            migrationBuilder.RenameColumn(
                name: "formData_245",
                table: "SW_forms",
                newName: "FormData245");

            migrationBuilder.RenameColumn(
                name: "formData_244",
                table: "SW_forms",
                newName: "FormData244");

            migrationBuilder.RenameColumn(
                name: "formData_243",
                table: "SW_forms",
                newName: "FormData243");

            migrationBuilder.RenameColumn(
                name: "formData_242",
                table: "SW_forms",
                newName: "FormData242");

            migrationBuilder.RenameColumn(
                name: "formData_241",
                table: "SW_forms",
                newName: "FormData241");

            migrationBuilder.RenameColumn(
                name: "formData_240",
                table: "SW_forms",
                newName: "FormData240");

            migrationBuilder.RenameColumn(
                name: "formData_24",
                table: "SW_forms",
                newName: "FormData24");

            migrationBuilder.RenameColumn(
                name: "formData_239",
                table: "SW_forms",
                newName: "FormData239");

            migrationBuilder.RenameColumn(
                name: "formData_238",
                table: "SW_forms",
                newName: "FormData238");

            migrationBuilder.RenameColumn(
                name: "formData_237",
                table: "SW_forms",
                newName: "FormData237");

            migrationBuilder.RenameColumn(
                name: "formData_236",
                table: "SW_forms",
                newName: "FormData236");

            migrationBuilder.RenameColumn(
                name: "formData_235",
                table: "SW_forms",
                newName: "FormData235");

            migrationBuilder.RenameColumn(
                name: "formData_234",
                table: "SW_forms",
                newName: "FormData234");

            migrationBuilder.RenameColumn(
                name: "formData_233",
                table: "SW_forms",
                newName: "FormData233");

            migrationBuilder.RenameColumn(
                name: "formData_232",
                table: "SW_forms",
                newName: "FormData232");

            migrationBuilder.RenameColumn(
                name: "formData_231",
                table: "SW_forms",
                newName: "FormData231");

            migrationBuilder.RenameColumn(
                name: "formData_230",
                table: "SW_forms",
                newName: "FormData230");

            migrationBuilder.RenameColumn(
                name: "formData_23",
                table: "SW_forms",
                newName: "FormData23");

            migrationBuilder.RenameColumn(
                name: "formData_229",
                table: "SW_forms",
                newName: "FormData229");

            migrationBuilder.RenameColumn(
                name: "formData_228",
                table: "SW_forms",
                newName: "FormData228");

            migrationBuilder.RenameColumn(
                name: "formData_227",
                table: "SW_forms",
                newName: "FormData227");

            migrationBuilder.RenameColumn(
                name: "formData_226",
                table: "SW_forms",
                newName: "FormData226");

            migrationBuilder.RenameColumn(
                name: "formData_225",
                table: "SW_forms",
                newName: "FormData225");

            migrationBuilder.RenameColumn(
                name: "formData_224",
                table: "SW_forms",
                newName: "FormData224");

            migrationBuilder.RenameColumn(
                name: "formData_223",
                table: "SW_forms",
                newName: "FormData223");

            migrationBuilder.RenameColumn(
                name: "formData_222",
                table: "SW_forms",
                newName: "FormData222");

            migrationBuilder.RenameColumn(
                name: "formData_221",
                table: "SW_forms",
                newName: "FormData221");

            migrationBuilder.RenameColumn(
                name: "formData_220",
                table: "SW_forms",
                newName: "FormData220");

            migrationBuilder.RenameColumn(
                name: "formData_22",
                table: "SW_forms",
                newName: "FormData22");

            migrationBuilder.RenameColumn(
                name: "formData_219",
                table: "SW_forms",
                newName: "FormData219");

            migrationBuilder.RenameColumn(
                name: "formData_218",
                table: "SW_forms",
                newName: "FormData218");

            migrationBuilder.RenameColumn(
                name: "formData_217",
                table: "SW_forms",
                newName: "FormData217");

            migrationBuilder.RenameColumn(
                name: "formData_216",
                table: "SW_forms",
                newName: "FormData216");

            migrationBuilder.RenameColumn(
                name: "formData_215",
                table: "SW_forms",
                newName: "FormData215");

            migrationBuilder.RenameColumn(
                name: "formData_214",
                table: "SW_forms",
                newName: "FormData214");

            migrationBuilder.RenameColumn(
                name: "formData_213",
                table: "SW_forms",
                newName: "FormData213");

            migrationBuilder.RenameColumn(
                name: "formData_212",
                table: "SW_forms",
                newName: "FormData212");

            migrationBuilder.RenameColumn(
                name: "formData_211",
                table: "SW_forms",
                newName: "FormData211");

            migrationBuilder.RenameColumn(
                name: "formData_210",
                table: "SW_forms",
                newName: "FormData210");

            migrationBuilder.RenameColumn(
                name: "formData_21",
                table: "SW_forms",
                newName: "FormData21");

            migrationBuilder.RenameColumn(
                name: "formData_209",
                table: "SW_forms",
                newName: "FormData209");

            migrationBuilder.RenameColumn(
                name: "formData_208",
                table: "SW_forms",
                newName: "FormData208");

            migrationBuilder.RenameColumn(
                name: "formData_207",
                table: "SW_forms",
                newName: "FormData207");

            migrationBuilder.RenameColumn(
                name: "formData_206",
                table: "SW_forms",
                newName: "FormData206");

            migrationBuilder.RenameColumn(
                name: "formData_205",
                table: "SW_forms",
                newName: "FormData205");

            migrationBuilder.RenameColumn(
                name: "formData_204",
                table: "SW_forms",
                newName: "FormData204");

            migrationBuilder.RenameColumn(
                name: "formData_203",
                table: "SW_forms",
                newName: "FormData203");

            migrationBuilder.RenameColumn(
                name: "formData_202",
                table: "SW_forms",
                newName: "FormData202");

            migrationBuilder.RenameColumn(
                name: "formData_201",
                table: "SW_forms",
                newName: "FormData201");

            migrationBuilder.RenameColumn(
                name: "formData_200",
                table: "SW_forms",
                newName: "FormData200");

            migrationBuilder.RenameColumn(
                name: "formData_20",
                table: "SW_forms",
                newName: "FormData20");

            migrationBuilder.RenameColumn(
                name: "formData_199",
                table: "SW_forms",
                newName: "FormData199");

            migrationBuilder.RenameColumn(
                name: "formData_198",
                table: "SW_forms",
                newName: "FormData198");

            migrationBuilder.RenameColumn(
                name: "formData_197",
                table: "SW_forms",
                newName: "FormData197");

            migrationBuilder.RenameColumn(
                name: "formData_196",
                table: "SW_forms",
                newName: "FormData196");

            migrationBuilder.RenameColumn(
                name: "formData_195",
                table: "SW_forms",
                newName: "FormData195");

            migrationBuilder.RenameColumn(
                name: "formData_194",
                table: "SW_forms",
                newName: "FormData194");

            migrationBuilder.RenameColumn(
                name: "formData_193",
                table: "SW_forms",
                newName: "FormData193");

            migrationBuilder.RenameColumn(
                name: "formData_192",
                table: "SW_forms",
                newName: "FormData192");

            migrationBuilder.RenameColumn(
                name: "formData_191",
                table: "SW_forms",
                newName: "FormData191");

            migrationBuilder.RenameColumn(
                name: "formData_190",
                table: "SW_forms",
                newName: "FormData190");

            migrationBuilder.RenameColumn(
                name: "formData_19",
                table: "SW_forms",
                newName: "FormData19");

            migrationBuilder.RenameColumn(
                name: "formData_189",
                table: "SW_forms",
                newName: "FormData189");

            migrationBuilder.RenameColumn(
                name: "formData_188",
                table: "SW_forms",
                newName: "FormData188");

            migrationBuilder.RenameColumn(
                name: "formData_187",
                table: "SW_forms",
                newName: "FormData187");

            migrationBuilder.RenameColumn(
                name: "formData_186",
                table: "SW_forms",
                newName: "FormData186");

            migrationBuilder.RenameColumn(
                name: "formData_185",
                table: "SW_forms",
                newName: "FormData185");

            migrationBuilder.RenameColumn(
                name: "formData_184",
                table: "SW_forms",
                newName: "FormData184");

            migrationBuilder.RenameColumn(
                name: "formData_183",
                table: "SW_forms",
                newName: "FormData183");

            migrationBuilder.RenameColumn(
                name: "formData_182",
                table: "SW_forms",
                newName: "FormData182");

            migrationBuilder.RenameColumn(
                name: "formData_181",
                table: "SW_forms",
                newName: "FormData181");

            migrationBuilder.RenameColumn(
                name: "formData_180",
                table: "SW_forms",
                newName: "FormData180");

            migrationBuilder.RenameColumn(
                name: "formData_18",
                table: "SW_forms",
                newName: "FormData18");

            migrationBuilder.RenameColumn(
                name: "formData_179",
                table: "SW_forms",
                newName: "FormData179");

            migrationBuilder.RenameColumn(
                name: "formData_178",
                table: "SW_forms",
                newName: "FormData178");

            migrationBuilder.RenameColumn(
                name: "formData_177",
                table: "SW_forms",
                newName: "FormData177");

            migrationBuilder.RenameColumn(
                name: "formData_176",
                table: "SW_forms",
                newName: "FormData176");

            migrationBuilder.RenameColumn(
                name: "formData_175",
                table: "SW_forms",
                newName: "FormData175");

            migrationBuilder.RenameColumn(
                name: "formData_174",
                table: "SW_forms",
                newName: "FormData174");

            migrationBuilder.RenameColumn(
                name: "formData_173",
                table: "SW_forms",
                newName: "FormData173");

            migrationBuilder.RenameColumn(
                name: "formData_172",
                table: "SW_forms",
                newName: "FormData172");

            migrationBuilder.RenameColumn(
                name: "formData_171",
                table: "SW_forms",
                newName: "FormData171");

            migrationBuilder.RenameColumn(
                name: "formData_170",
                table: "SW_forms",
                newName: "FormData170");

            migrationBuilder.RenameColumn(
                name: "formData_17",
                table: "SW_forms",
                newName: "FormData17");

            migrationBuilder.RenameColumn(
                name: "formData_169",
                table: "SW_forms",
                newName: "FormData169");

            migrationBuilder.RenameColumn(
                name: "formData_168",
                table: "SW_forms",
                newName: "FormData168");

            migrationBuilder.RenameColumn(
                name: "formData_167",
                table: "SW_forms",
                newName: "FormData167");

            migrationBuilder.RenameColumn(
                name: "formData_166",
                table: "SW_forms",
                newName: "FormData166");

            migrationBuilder.RenameColumn(
                name: "formData_165",
                table: "SW_forms",
                newName: "FormData165");

            migrationBuilder.RenameColumn(
                name: "formData_164",
                table: "SW_forms",
                newName: "FormData164");

            migrationBuilder.RenameColumn(
                name: "formData_163",
                table: "SW_forms",
                newName: "FormData163");

            migrationBuilder.RenameColumn(
                name: "formData_162",
                table: "SW_forms",
                newName: "FormData162");

            migrationBuilder.RenameColumn(
                name: "formData_161",
                table: "SW_forms",
                newName: "FormData161");

            migrationBuilder.RenameColumn(
                name: "formData_160",
                table: "SW_forms",
                newName: "FormData160");

            migrationBuilder.RenameColumn(
                name: "formData_16",
                table: "SW_forms",
                newName: "FormData16");

            migrationBuilder.RenameColumn(
                name: "formData_159",
                table: "SW_forms",
                newName: "FormData159");

            migrationBuilder.RenameColumn(
                name: "formData_158",
                table: "SW_forms",
                newName: "FormData158");

            migrationBuilder.RenameColumn(
                name: "formData_157",
                table: "SW_forms",
                newName: "FormData157");

            migrationBuilder.RenameColumn(
                name: "formData_156",
                table: "SW_forms",
                newName: "FormData156");

            migrationBuilder.RenameColumn(
                name: "formData_155",
                table: "SW_forms",
                newName: "FormData155");

            migrationBuilder.RenameColumn(
                name: "formData_154",
                table: "SW_forms",
                newName: "FormData154");

            migrationBuilder.RenameColumn(
                name: "formData_153",
                table: "SW_forms",
                newName: "FormData153");

            migrationBuilder.RenameColumn(
                name: "formData_152",
                table: "SW_forms",
                newName: "FormData152");

            migrationBuilder.RenameColumn(
                name: "formData_151",
                table: "SW_forms",
                newName: "FormData151");

            migrationBuilder.RenameColumn(
                name: "formData_150",
                table: "SW_forms",
                newName: "FormData150");

            migrationBuilder.RenameColumn(
                name: "formData_15",
                table: "SW_forms",
                newName: "FormData15");

            migrationBuilder.RenameColumn(
                name: "formData_149",
                table: "SW_forms",
                newName: "FormData149");

            migrationBuilder.RenameColumn(
                name: "formData_148",
                table: "SW_forms",
                newName: "FormData148");

            migrationBuilder.RenameColumn(
                name: "formData_147",
                table: "SW_forms",
                newName: "FormData147");

            migrationBuilder.RenameColumn(
                name: "formData_146",
                table: "SW_forms",
                newName: "FormData146");

            migrationBuilder.RenameColumn(
                name: "formData_145",
                table: "SW_forms",
                newName: "FormData145");

            migrationBuilder.RenameColumn(
                name: "formData_144",
                table: "SW_forms",
                newName: "FormData144");

            migrationBuilder.RenameColumn(
                name: "formData_143",
                table: "SW_forms",
                newName: "FormData143");

            migrationBuilder.RenameColumn(
                name: "formData_142",
                table: "SW_forms",
                newName: "FormData142");

            migrationBuilder.RenameColumn(
                name: "formData_141",
                table: "SW_forms",
                newName: "FormData141");

            migrationBuilder.RenameColumn(
                name: "formData_140",
                table: "SW_forms",
                newName: "FormData140");

            migrationBuilder.RenameColumn(
                name: "formData_14",
                table: "SW_forms",
                newName: "FormData14");

            migrationBuilder.RenameColumn(
                name: "formData_139",
                table: "SW_forms",
                newName: "FormData139");

            migrationBuilder.RenameColumn(
                name: "formData_138",
                table: "SW_forms",
                newName: "FormData138");

            migrationBuilder.RenameColumn(
                name: "formData_137",
                table: "SW_forms",
                newName: "FormData137");

            migrationBuilder.RenameColumn(
                name: "formData_136",
                table: "SW_forms",
                newName: "FormData136");

            migrationBuilder.RenameColumn(
                name: "formData_135",
                table: "SW_forms",
                newName: "FormData135");

            migrationBuilder.RenameColumn(
                name: "formData_134",
                table: "SW_forms",
                newName: "FormData134");

            migrationBuilder.RenameColumn(
                name: "formData_133",
                table: "SW_forms",
                newName: "FormData133");

            migrationBuilder.RenameColumn(
                name: "formData_132",
                table: "SW_forms",
                newName: "FormData132");

            migrationBuilder.RenameColumn(
                name: "formData_131",
                table: "SW_forms",
                newName: "FormData131");

            migrationBuilder.RenameColumn(
                name: "formData_130",
                table: "SW_forms",
                newName: "FormData130");

            migrationBuilder.RenameColumn(
                name: "formData_13",
                table: "SW_forms",
                newName: "FormData13");

            migrationBuilder.RenameColumn(
                name: "formData_129",
                table: "SW_forms",
                newName: "FormData129");

            migrationBuilder.RenameColumn(
                name: "formData_128",
                table: "SW_forms",
                newName: "FormData128");

            migrationBuilder.RenameColumn(
                name: "formData_127",
                table: "SW_forms",
                newName: "FormData127");

            migrationBuilder.RenameColumn(
                name: "formData_126",
                table: "SW_forms",
                newName: "FormData126");

            migrationBuilder.RenameColumn(
                name: "formData_125",
                table: "SW_forms",
                newName: "FormData125");

            migrationBuilder.RenameColumn(
                name: "formData_124",
                table: "SW_forms",
                newName: "FormData124");

            migrationBuilder.RenameColumn(
                name: "formData_123",
                table: "SW_forms",
                newName: "FormData123");

            migrationBuilder.RenameColumn(
                name: "formData_122",
                table: "SW_forms",
                newName: "FormData122");

            migrationBuilder.RenameColumn(
                name: "formData_121",
                table: "SW_forms",
                newName: "FormData121");

            migrationBuilder.RenameColumn(
                name: "formData_120",
                table: "SW_forms",
                newName: "FormData120");

            migrationBuilder.RenameColumn(
                name: "formData_12",
                table: "SW_forms",
                newName: "FormData12");

            migrationBuilder.RenameColumn(
                name: "formData_119",
                table: "SW_forms",
                newName: "FormData119");

            migrationBuilder.RenameColumn(
                name: "formData_118",
                table: "SW_forms",
                newName: "FormData118");

            migrationBuilder.RenameColumn(
                name: "formData_117",
                table: "SW_forms",
                newName: "FormData117");

            migrationBuilder.RenameColumn(
                name: "formData_116",
                table: "SW_forms",
                newName: "FormData116");

            migrationBuilder.RenameColumn(
                name: "formData_115",
                table: "SW_forms",
                newName: "FormData115");

            migrationBuilder.RenameColumn(
                name: "formData_114",
                table: "SW_forms",
                newName: "FormData114");

            migrationBuilder.RenameColumn(
                name: "formData_113",
                table: "SW_forms",
                newName: "FormData113");

            migrationBuilder.RenameColumn(
                name: "formData_112",
                table: "SW_forms",
                newName: "FormData112");

            migrationBuilder.RenameColumn(
                name: "formData_111",
                table: "SW_forms",
                newName: "FormData111");

            migrationBuilder.RenameColumn(
                name: "formData_110",
                table: "SW_forms",
                newName: "FormData110");

            migrationBuilder.RenameColumn(
                name: "formData_11",
                table: "SW_forms",
                newName: "FormData11");

            migrationBuilder.RenameColumn(
                name: "formData_109",
                table: "SW_forms",
                newName: "FormData109");

            migrationBuilder.RenameColumn(
                name: "formData_108",
                table: "SW_forms",
                newName: "FormData108");

            migrationBuilder.RenameColumn(
                name: "formData_107",
                table: "SW_forms",
                newName: "FormData107");

            migrationBuilder.RenameColumn(
                name: "formData_106",
                table: "SW_forms",
                newName: "FormData106");

            migrationBuilder.RenameColumn(
                name: "formData_105",
                table: "SW_forms",
                newName: "FormData105");

            migrationBuilder.RenameColumn(
                name: "formData_104",
                table: "SW_forms",
                newName: "FormData104");

            migrationBuilder.RenameColumn(
                name: "formData_103",
                table: "SW_forms",
                newName: "FormData103");

            migrationBuilder.RenameColumn(
                name: "formData_102",
                table: "SW_forms",
                newName: "FormData102");

            migrationBuilder.RenameColumn(
                name: "formData_101",
                table: "SW_forms",
                newName: "FormData101");

            migrationBuilder.RenameColumn(
                name: "formData_100",
                table: "SW_forms",
                newName: "FormData100");

            migrationBuilder.RenameColumn(
                name: "formData_10",
                table: "SW_forms",
                newName: "FormData10");

            migrationBuilder.RenameColumn(
                name: "formData_09",
                table: "SW_forms",
                newName: "FormData09");

            migrationBuilder.RenameColumn(
                name: "formData_08",
                table: "SW_forms",
                newName: "FormData08");

            migrationBuilder.RenameColumn(
                name: "formData_07",
                table: "SW_forms",
                newName: "FormData07");

            migrationBuilder.RenameColumn(
                name: "formData_06",
                table: "SW_forms",
                newName: "FormData06");

            migrationBuilder.RenameColumn(
                name: "formData_05",
                table: "SW_forms",
                newName: "FormData05");

            migrationBuilder.RenameColumn(
                name: "formData_04",
                table: "SW_forms",
                newName: "FormData04");

            migrationBuilder.RenameColumn(
                name: "formData_03",
                table: "SW_forms",
                newName: "FormData03");

            migrationBuilder.RenameColumn(
                name: "formData_02",
                table: "SW_forms",
                newName: "FormData02");

            migrationBuilder.RenameColumn(
                name: "formData_01",
                table: "SW_forms",
                newName: "FormData01");

            migrationBuilder.AlterColumn<string>(
                name: "media_03",
                table: "SW_identity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "media_02",
                table: "SW_identity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "media_01",
                table: "SW_identity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "uuid",
                table: "SW_forms",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "form",
                table: "SW_forms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "desc",
                table: "SW_forms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isApproval_03",
                table: "SW_forms",
                newName: "is_approval_03");

            migrationBuilder.RenameColumn(
                name: "isApproval_02",
                table: "SW_forms",
                newName: "is_approval_02");

            migrationBuilder.RenameColumn(
                name: "isApproval_01",
                table: "SW_forms",
                newName: "is_approval_01");

            migrationBuilder.RenameColumn(
                name: "FormData99",
                table: "SW_forms",
                newName: "formData_99");

            migrationBuilder.RenameColumn(
                name: "FormData98",
                table: "SW_forms",
                newName: "formData_98");

            migrationBuilder.RenameColumn(
                name: "FormData97",
                table: "SW_forms",
                newName: "formData_97");

            migrationBuilder.RenameColumn(
                name: "FormData96",
                table: "SW_forms",
                newName: "formData_96");

            migrationBuilder.RenameColumn(
                name: "FormData95",
                table: "SW_forms",
                newName: "formData_95");

            migrationBuilder.RenameColumn(
                name: "FormData94",
                table: "SW_forms",
                newName: "formData_94");

            migrationBuilder.RenameColumn(
                name: "FormData93",
                table: "SW_forms",
                newName: "formData_93");

            migrationBuilder.RenameColumn(
                name: "FormData92",
                table: "SW_forms",
                newName: "formData_92");

            migrationBuilder.RenameColumn(
                name: "FormData91",
                table: "SW_forms",
                newName: "formData_91");

            migrationBuilder.RenameColumn(
                name: "FormData90",
                table: "SW_forms",
                newName: "formData_90");

            migrationBuilder.RenameColumn(
                name: "FormData89",
                table: "SW_forms",
                newName: "formData_89");

            migrationBuilder.RenameColumn(
                name: "FormData88",
                table: "SW_forms",
                newName: "formData_88");

            migrationBuilder.RenameColumn(
                name: "FormData87",
                table: "SW_forms",
                newName: "formData_87");

            migrationBuilder.RenameColumn(
                name: "FormData86",
                table: "SW_forms",
                newName: "formData_86");

            migrationBuilder.RenameColumn(
                name: "FormData85",
                table: "SW_forms",
                newName: "formData_85");

            migrationBuilder.RenameColumn(
                name: "FormData84",
                table: "SW_forms",
                newName: "formData_84");

            migrationBuilder.RenameColumn(
                name: "FormData83",
                table: "SW_forms",
                newName: "formData_83");

            migrationBuilder.RenameColumn(
                name: "FormData82",
                table: "SW_forms",
                newName: "formData_82");

            migrationBuilder.RenameColumn(
                name: "FormData81",
                table: "SW_forms",
                newName: "formData_81");

            migrationBuilder.RenameColumn(
                name: "FormData80",
                table: "SW_forms",
                newName: "formData_80");

            migrationBuilder.RenameColumn(
                name: "FormData79",
                table: "SW_forms",
                newName: "formData_79");

            migrationBuilder.RenameColumn(
                name: "FormData78",
                table: "SW_forms",
                newName: "formData_78");

            migrationBuilder.RenameColumn(
                name: "FormData77",
                table: "SW_forms",
                newName: "formData_77");

            migrationBuilder.RenameColumn(
                name: "FormData76",
                table: "SW_forms",
                newName: "formData_76");

            migrationBuilder.RenameColumn(
                name: "FormData75",
                table: "SW_forms",
                newName: "formData_75");

            migrationBuilder.RenameColumn(
                name: "FormData74",
                table: "SW_forms",
                newName: "formData_74");

            migrationBuilder.RenameColumn(
                name: "FormData73",
                table: "SW_forms",
                newName: "formData_73");

            migrationBuilder.RenameColumn(
                name: "FormData72",
                table: "SW_forms",
                newName: "formData_72");

            migrationBuilder.RenameColumn(
                name: "FormData71",
                table: "SW_forms",
                newName: "formData_71");

            migrationBuilder.RenameColumn(
                name: "FormData70",
                table: "SW_forms",
                newName: "formData_70");

            migrationBuilder.RenameColumn(
                name: "FormData69",
                table: "SW_forms",
                newName: "formData_69");

            migrationBuilder.RenameColumn(
                name: "FormData68",
                table: "SW_forms",
                newName: "formData_68");

            migrationBuilder.RenameColumn(
                name: "FormData67",
                table: "SW_forms",
                newName: "formData_67");

            migrationBuilder.RenameColumn(
                name: "FormData66",
                table: "SW_forms",
                newName: "formData_66");

            migrationBuilder.RenameColumn(
                name: "FormData65",
                table: "SW_forms",
                newName: "formData_65");

            migrationBuilder.RenameColumn(
                name: "FormData64",
                table: "SW_forms",
                newName: "formData_64");

            migrationBuilder.RenameColumn(
                name: "FormData63",
                table: "SW_forms",
                newName: "formData_63");

            migrationBuilder.RenameColumn(
                name: "FormData62",
                table: "SW_forms",
                newName: "formData_62");

            migrationBuilder.RenameColumn(
                name: "FormData61",
                table: "SW_forms",
                newName: "formData_61");

            migrationBuilder.RenameColumn(
                name: "FormData60",
                table: "SW_forms",
                newName: "formData_60");

            migrationBuilder.RenameColumn(
                name: "FormData59",
                table: "SW_forms",
                newName: "formData_59");

            migrationBuilder.RenameColumn(
                name: "FormData58",
                table: "SW_forms",
                newName: "formData_58");

            migrationBuilder.RenameColumn(
                name: "FormData57",
                table: "SW_forms",
                newName: "formData_57");

            migrationBuilder.RenameColumn(
                name: "FormData56",
                table: "SW_forms",
                newName: "formData_56");

            migrationBuilder.RenameColumn(
                name: "FormData55",
                table: "SW_forms",
                newName: "formData_55");

            migrationBuilder.RenameColumn(
                name: "FormData54",
                table: "SW_forms",
                newName: "formData_54");

            migrationBuilder.RenameColumn(
                name: "FormData53",
                table: "SW_forms",
                newName: "formData_53");

            migrationBuilder.RenameColumn(
                name: "FormData52",
                table: "SW_forms",
                newName: "formData_52");

            migrationBuilder.RenameColumn(
                name: "FormData51",
                table: "SW_forms",
                newName: "formData_51");

            migrationBuilder.RenameColumn(
                name: "FormData50",
                table: "SW_forms",
                newName: "formData_50");

            migrationBuilder.RenameColumn(
                name: "FormData49",
                table: "SW_forms",
                newName: "formData_49");

            migrationBuilder.RenameColumn(
                name: "FormData48",
                table: "SW_forms",
                newName: "formData_48");

            migrationBuilder.RenameColumn(
                name: "FormData47",
                table: "SW_forms",
                newName: "formData_47");

            migrationBuilder.RenameColumn(
                name: "FormData46",
                table: "SW_forms",
                newName: "formData_46");

            migrationBuilder.RenameColumn(
                name: "FormData45",
                table: "SW_forms",
                newName: "formData_45");

            migrationBuilder.RenameColumn(
                name: "FormData44",
                table: "SW_forms",
                newName: "formData_44");

            migrationBuilder.RenameColumn(
                name: "FormData43",
                table: "SW_forms",
                newName: "formData_43");

            migrationBuilder.RenameColumn(
                name: "FormData42",
                table: "SW_forms",
                newName: "formData_42");

            migrationBuilder.RenameColumn(
                name: "FormData41",
                table: "SW_forms",
                newName: "formData_41");

            migrationBuilder.RenameColumn(
                name: "FormData40",
                table: "SW_forms",
                newName: "formData_40");

            migrationBuilder.RenameColumn(
                name: "FormData39",
                table: "SW_forms",
                newName: "formData_39");

            migrationBuilder.RenameColumn(
                name: "FormData38",
                table: "SW_forms",
                newName: "formData_38");

            migrationBuilder.RenameColumn(
                name: "FormData37",
                table: "SW_forms",
                newName: "formData_37");

            migrationBuilder.RenameColumn(
                name: "FormData36",
                table: "SW_forms",
                newName: "formData_36");

            migrationBuilder.RenameColumn(
                name: "FormData35",
                table: "SW_forms",
                newName: "formData_35");

            migrationBuilder.RenameColumn(
                name: "FormData34",
                table: "SW_forms",
                newName: "formData_34");

            migrationBuilder.RenameColumn(
                name: "FormData33",
                table: "SW_forms",
                newName: "formData_33");

            migrationBuilder.RenameColumn(
                name: "FormData32",
                table: "SW_forms",
                newName: "formData_32");

            migrationBuilder.RenameColumn(
                name: "FormData31",
                table: "SW_forms",
                newName: "formData_31");

            migrationBuilder.RenameColumn(
                name: "FormData30",
                table: "SW_forms",
                newName: "formData_30");

            migrationBuilder.RenameColumn(
                name: "FormData29",
                table: "SW_forms",
                newName: "formData_29");

            migrationBuilder.RenameColumn(
                name: "FormData28",
                table: "SW_forms",
                newName: "formData_28");

            migrationBuilder.RenameColumn(
                name: "FormData27",
                table: "SW_forms",
                newName: "formData_27");

            migrationBuilder.RenameColumn(
                name: "FormData26",
                table: "SW_forms",
                newName: "formData_26");

            migrationBuilder.RenameColumn(
                name: "FormData250",
                table: "SW_forms",
                newName: "formData_250");

            migrationBuilder.RenameColumn(
                name: "FormData25",
                table: "SW_forms",
                newName: "formData_25");

            migrationBuilder.RenameColumn(
                name: "FormData249",
                table: "SW_forms",
                newName: "formData_249");

            migrationBuilder.RenameColumn(
                name: "FormData248",
                table: "SW_forms",
                newName: "formData_248");

            migrationBuilder.RenameColumn(
                name: "FormData247",
                table: "SW_forms",
                newName: "formData_247");

            migrationBuilder.RenameColumn(
                name: "FormData246",
                table: "SW_forms",
                newName: "formData_246");

            migrationBuilder.RenameColumn(
                name: "FormData245",
                table: "SW_forms",
                newName: "formData_245");

            migrationBuilder.RenameColumn(
                name: "FormData244",
                table: "SW_forms",
                newName: "formData_244");

            migrationBuilder.RenameColumn(
                name: "FormData243",
                table: "SW_forms",
                newName: "formData_243");

            migrationBuilder.RenameColumn(
                name: "FormData242",
                table: "SW_forms",
                newName: "formData_242");

            migrationBuilder.RenameColumn(
                name: "FormData241",
                table: "SW_forms",
                newName: "formData_241");

            migrationBuilder.RenameColumn(
                name: "FormData240",
                table: "SW_forms",
                newName: "formData_240");

            migrationBuilder.RenameColumn(
                name: "FormData24",
                table: "SW_forms",
                newName: "formData_24");

            migrationBuilder.RenameColumn(
                name: "FormData239",
                table: "SW_forms",
                newName: "formData_239");

            migrationBuilder.RenameColumn(
                name: "FormData238",
                table: "SW_forms",
                newName: "formData_238");

            migrationBuilder.RenameColumn(
                name: "FormData237",
                table: "SW_forms",
                newName: "formData_237");

            migrationBuilder.RenameColumn(
                name: "FormData236",
                table: "SW_forms",
                newName: "formData_236");

            migrationBuilder.RenameColumn(
                name: "FormData235",
                table: "SW_forms",
                newName: "formData_235");

            migrationBuilder.RenameColumn(
                name: "FormData234",
                table: "SW_forms",
                newName: "formData_234");

            migrationBuilder.RenameColumn(
                name: "FormData233",
                table: "SW_forms",
                newName: "formData_233");

            migrationBuilder.RenameColumn(
                name: "FormData232",
                table: "SW_forms",
                newName: "formData_232");

            migrationBuilder.RenameColumn(
                name: "FormData231",
                table: "SW_forms",
                newName: "formData_231");

            migrationBuilder.RenameColumn(
                name: "FormData230",
                table: "SW_forms",
                newName: "formData_230");

            migrationBuilder.RenameColumn(
                name: "FormData23",
                table: "SW_forms",
                newName: "formData_23");

            migrationBuilder.RenameColumn(
                name: "FormData229",
                table: "SW_forms",
                newName: "formData_229");

            migrationBuilder.RenameColumn(
                name: "FormData228",
                table: "SW_forms",
                newName: "formData_228");

            migrationBuilder.RenameColumn(
                name: "FormData227",
                table: "SW_forms",
                newName: "formData_227");

            migrationBuilder.RenameColumn(
                name: "FormData226",
                table: "SW_forms",
                newName: "formData_226");

            migrationBuilder.RenameColumn(
                name: "FormData225",
                table: "SW_forms",
                newName: "formData_225");

            migrationBuilder.RenameColumn(
                name: "FormData224",
                table: "SW_forms",
                newName: "formData_224");

            migrationBuilder.RenameColumn(
                name: "FormData223",
                table: "SW_forms",
                newName: "formData_223");

            migrationBuilder.RenameColumn(
                name: "FormData222",
                table: "SW_forms",
                newName: "formData_222");

            migrationBuilder.RenameColumn(
                name: "FormData221",
                table: "SW_forms",
                newName: "formData_221");

            migrationBuilder.RenameColumn(
                name: "FormData220",
                table: "SW_forms",
                newName: "formData_220");

            migrationBuilder.RenameColumn(
                name: "FormData22",
                table: "SW_forms",
                newName: "formData_22");

            migrationBuilder.RenameColumn(
                name: "FormData219",
                table: "SW_forms",
                newName: "formData_219");

            migrationBuilder.RenameColumn(
                name: "FormData218",
                table: "SW_forms",
                newName: "formData_218");

            migrationBuilder.RenameColumn(
                name: "FormData217",
                table: "SW_forms",
                newName: "formData_217");

            migrationBuilder.RenameColumn(
                name: "FormData216",
                table: "SW_forms",
                newName: "formData_216");

            migrationBuilder.RenameColumn(
                name: "FormData215",
                table: "SW_forms",
                newName: "formData_215");

            migrationBuilder.RenameColumn(
                name: "FormData214",
                table: "SW_forms",
                newName: "formData_214");

            migrationBuilder.RenameColumn(
                name: "FormData213",
                table: "SW_forms",
                newName: "formData_213");

            migrationBuilder.RenameColumn(
                name: "FormData212",
                table: "SW_forms",
                newName: "formData_212");

            migrationBuilder.RenameColumn(
                name: "FormData211",
                table: "SW_forms",
                newName: "formData_211");

            migrationBuilder.RenameColumn(
                name: "FormData210",
                table: "SW_forms",
                newName: "formData_210");

            migrationBuilder.RenameColumn(
                name: "FormData21",
                table: "SW_forms",
                newName: "formData_21");

            migrationBuilder.RenameColumn(
                name: "FormData209",
                table: "SW_forms",
                newName: "formData_209");

            migrationBuilder.RenameColumn(
                name: "FormData208",
                table: "SW_forms",
                newName: "formData_208");

            migrationBuilder.RenameColumn(
                name: "FormData207",
                table: "SW_forms",
                newName: "formData_207");

            migrationBuilder.RenameColumn(
                name: "FormData206",
                table: "SW_forms",
                newName: "formData_206");

            migrationBuilder.RenameColumn(
                name: "FormData205",
                table: "SW_forms",
                newName: "formData_205");

            migrationBuilder.RenameColumn(
                name: "FormData204",
                table: "SW_forms",
                newName: "formData_204");

            migrationBuilder.RenameColumn(
                name: "FormData203",
                table: "SW_forms",
                newName: "formData_203");

            migrationBuilder.RenameColumn(
                name: "FormData202",
                table: "SW_forms",
                newName: "formData_202");

            migrationBuilder.RenameColumn(
                name: "FormData201",
                table: "SW_forms",
                newName: "formData_201");

            migrationBuilder.RenameColumn(
                name: "FormData200",
                table: "SW_forms",
                newName: "formData_200");

            migrationBuilder.RenameColumn(
                name: "FormData20",
                table: "SW_forms",
                newName: "formData_20");

            migrationBuilder.RenameColumn(
                name: "FormData199",
                table: "SW_forms",
                newName: "formData_199");

            migrationBuilder.RenameColumn(
                name: "FormData198",
                table: "SW_forms",
                newName: "formData_198");

            migrationBuilder.RenameColumn(
                name: "FormData197",
                table: "SW_forms",
                newName: "formData_197");

            migrationBuilder.RenameColumn(
                name: "FormData196",
                table: "SW_forms",
                newName: "formData_196");

            migrationBuilder.RenameColumn(
                name: "FormData195",
                table: "SW_forms",
                newName: "formData_195");

            migrationBuilder.RenameColumn(
                name: "FormData194",
                table: "SW_forms",
                newName: "formData_194");

            migrationBuilder.RenameColumn(
                name: "FormData193",
                table: "SW_forms",
                newName: "formData_193");

            migrationBuilder.RenameColumn(
                name: "FormData192",
                table: "SW_forms",
                newName: "formData_192");

            migrationBuilder.RenameColumn(
                name: "FormData191",
                table: "SW_forms",
                newName: "formData_191");

            migrationBuilder.RenameColumn(
                name: "FormData190",
                table: "SW_forms",
                newName: "formData_190");

            migrationBuilder.RenameColumn(
                name: "FormData19",
                table: "SW_forms",
                newName: "formData_19");

            migrationBuilder.RenameColumn(
                name: "FormData189",
                table: "SW_forms",
                newName: "formData_189");

            migrationBuilder.RenameColumn(
                name: "FormData188",
                table: "SW_forms",
                newName: "formData_188");

            migrationBuilder.RenameColumn(
                name: "FormData187",
                table: "SW_forms",
                newName: "formData_187");

            migrationBuilder.RenameColumn(
                name: "FormData186",
                table: "SW_forms",
                newName: "formData_186");

            migrationBuilder.RenameColumn(
                name: "FormData185",
                table: "SW_forms",
                newName: "formData_185");

            migrationBuilder.RenameColumn(
                name: "FormData184",
                table: "SW_forms",
                newName: "formData_184");

            migrationBuilder.RenameColumn(
                name: "FormData183",
                table: "SW_forms",
                newName: "formData_183");

            migrationBuilder.RenameColumn(
                name: "FormData182",
                table: "SW_forms",
                newName: "formData_182");

            migrationBuilder.RenameColumn(
                name: "FormData181",
                table: "SW_forms",
                newName: "formData_181");

            migrationBuilder.RenameColumn(
                name: "FormData180",
                table: "SW_forms",
                newName: "formData_180");

            migrationBuilder.RenameColumn(
                name: "FormData18",
                table: "SW_forms",
                newName: "formData_18");

            migrationBuilder.RenameColumn(
                name: "FormData179",
                table: "SW_forms",
                newName: "formData_179");

            migrationBuilder.RenameColumn(
                name: "FormData178",
                table: "SW_forms",
                newName: "formData_178");

            migrationBuilder.RenameColumn(
                name: "FormData177",
                table: "SW_forms",
                newName: "formData_177");

            migrationBuilder.RenameColumn(
                name: "FormData176",
                table: "SW_forms",
                newName: "formData_176");

            migrationBuilder.RenameColumn(
                name: "FormData175",
                table: "SW_forms",
                newName: "formData_175");

            migrationBuilder.RenameColumn(
                name: "FormData174",
                table: "SW_forms",
                newName: "formData_174");

            migrationBuilder.RenameColumn(
                name: "FormData173",
                table: "SW_forms",
                newName: "formData_173");

            migrationBuilder.RenameColumn(
                name: "FormData172",
                table: "SW_forms",
                newName: "formData_172");

            migrationBuilder.RenameColumn(
                name: "FormData171",
                table: "SW_forms",
                newName: "formData_171");

            migrationBuilder.RenameColumn(
                name: "FormData170",
                table: "SW_forms",
                newName: "formData_170");

            migrationBuilder.RenameColumn(
                name: "FormData17",
                table: "SW_forms",
                newName: "formData_17");

            migrationBuilder.RenameColumn(
                name: "FormData169",
                table: "SW_forms",
                newName: "formData_169");

            migrationBuilder.RenameColumn(
                name: "FormData168",
                table: "SW_forms",
                newName: "formData_168");

            migrationBuilder.RenameColumn(
                name: "FormData167",
                table: "SW_forms",
                newName: "formData_167");

            migrationBuilder.RenameColumn(
                name: "FormData166",
                table: "SW_forms",
                newName: "formData_166");

            migrationBuilder.RenameColumn(
                name: "FormData165",
                table: "SW_forms",
                newName: "formData_165");

            migrationBuilder.RenameColumn(
                name: "FormData164",
                table: "SW_forms",
                newName: "formData_164");

            migrationBuilder.RenameColumn(
                name: "FormData163",
                table: "SW_forms",
                newName: "formData_163");

            migrationBuilder.RenameColumn(
                name: "FormData162",
                table: "SW_forms",
                newName: "formData_162");

            migrationBuilder.RenameColumn(
                name: "FormData161",
                table: "SW_forms",
                newName: "formData_161");

            migrationBuilder.RenameColumn(
                name: "FormData160",
                table: "SW_forms",
                newName: "formData_160");

            migrationBuilder.RenameColumn(
                name: "FormData16",
                table: "SW_forms",
                newName: "formData_16");

            migrationBuilder.RenameColumn(
                name: "FormData159",
                table: "SW_forms",
                newName: "formData_159");

            migrationBuilder.RenameColumn(
                name: "FormData158",
                table: "SW_forms",
                newName: "formData_158");

            migrationBuilder.RenameColumn(
                name: "FormData157",
                table: "SW_forms",
                newName: "formData_157");

            migrationBuilder.RenameColumn(
                name: "FormData156",
                table: "SW_forms",
                newName: "formData_156");

            migrationBuilder.RenameColumn(
                name: "FormData155",
                table: "SW_forms",
                newName: "formData_155");

            migrationBuilder.RenameColumn(
                name: "FormData154",
                table: "SW_forms",
                newName: "formData_154");

            migrationBuilder.RenameColumn(
                name: "FormData153",
                table: "SW_forms",
                newName: "formData_153");

            migrationBuilder.RenameColumn(
                name: "FormData152",
                table: "SW_forms",
                newName: "formData_152");

            migrationBuilder.RenameColumn(
                name: "FormData151",
                table: "SW_forms",
                newName: "formData_151");

            migrationBuilder.RenameColumn(
                name: "FormData150",
                table: "SW_forms",
                newName: "formData_150");

            migrationBuilder.RenameColumn(
                name: "FormData15",
                table: "SW_forms",
                newName: "formData_15");

            migrationBuilder.RenameColumn(
                name: "FormData149",
                table: "SW_forms",
                newName: "formData_149");

            migrationBuilder.RenameColumn(
                name: "FormData148",
                table: "SW_forms",
                newName: "formData_148");

            migrationBuilder.RenameColumn(
                name: "FormData147",
                table: "SW_forms",
                newName: "formData_147");

            migrationBuilder.RenameColumn(
                name: "FormData146",
                table: "SW_forms",
                newName: "formData_146");

            migrationBuilder.RenameColumn(
                name: "FormData145",
                table: "SW_forms",
                newName: "formData_145");

            migrationBuilder.RenameColumn(
                name: "FormData144",
                table: "SW_forms",
                newName: "formData_144");

            migrationBuilder.RenameColumn(
                name: "FormData143",
                table: "SW_forms",
                newName: "formData_143");

            migrationBuilder.RenameColumn(
                name: "FormData142",
                table: "SW_forms",
                newName: "formData_142");

            migrationBuilder.RenameColumn(
                name: "FormData141",
                table: "SW_forms",
                newName: "formData_141");

            migrationBuilder.RenameColumn(
                name: "FormData140",
                table: "SW_forms",
                newName: "formData_140");

            migrationBuilder.RenameColumn(
                name: "FormData14",
                table: "SW_forms",
                newName: "formData_14");

            migrationBuilder.RenameColumn(
                name: "FormData139",
                table: "SW_forms",
                newName: "formData_139");

            migrationBuilder.RenameColumn(
                name: "FormData138",
                table: "SW_forms",
                newName: "formData_138");

            migrationBuilder.RenameColumn(
                name: "FormData137",
                table: "SW_forms",
                newName: "formData_137");

            migrationBuilder.RenameColumn(
                name: "FormData136",
                table: "SW_forms",
                newName: "formData_136");

            migrationBuilder.RenameColumn(
                name: "FormData135",
                table: "SW_forms",
                newName: "formData_135");

            migrationBuilder.RenameColumn(
                name: "FormData134",
                table: "SW_forms",
                newName: "formData_134");

            migrationBuilder.RenameColumn(
                name: "FormData133",
                table: "SW_forms",
                newName: "formData_133");

            migrationBuilder.RenameColumn(
                name: "FormData132",
                table: "SW_forms",
                newName: "formData_132");

            migrationBuilder.RenameColumn(
                name: "FormData131",
                table: "SW_forms",
                newName: "formData_131");

            migrationBuilder.RenameColumn(
                name: "FormData130",
                table: "SW_forms",
                newName: "formData_130");

            migrationBuilder.RenameColumn(
                name: "FormData13",
                table: "SW_forms",
                newName: "formData_13");

            migrationBuilder.RenameColumn(
                name: "FormData129",
                table: "SW_forms",
                newName: "formData_129");

            migrationBuilder.RenameColumn(
                name: "FormData128",
                table: "SW_forms",
                newName: "formData_128");

            migrationBuilder.RenameColumn(
                name: "FormData127",
                table: "SW_forms",
                newName: "formData_127");

            migrationBuilder.RenameColumn(
                name: "FormData126",
                table: "SW_forms",
                newName: "formData_126");

            migrationBuilder.RenameColumn(
                name: "FormData125",
                table: "SW_forms",
                newName: "formData_125");

            migrationBuilder.RenameColumn(
                name: "FormData124",
                table: "SW_forms",
                newName: "formData_124");

            migrationBuilder.RenameColumn(
                name: "FormData123",
                table: "SW_forms",
                newName: "formData_123");

            migrationBuilder.RenameColumn(
                name: "FormData122",
                table: "SW_forms",
                newName: "formData_122");

            migrationBuilder.RenameColumn(
                name: "FormData121",
                table: "SW_forms",
                newName: "formData_121");

            migrationBuilder.RenameColumn(
                name: "FormData120",
                table: "SW_forms",
                newName: "formData_120");

            migrationBuilder.RenameColumn(
                name: "FormData12",
                table: "SW_forms",
                newName: "formData_12");

            migrationBuilder.RenameColumn(
                name: "FormData119",
                table: "SW_forms",
                newName: "formData_119");

            migrationBuilder.RenameColumn(
                name: "FormData118",
                table: "SW_forms",
                newName: "formData_118");

            migrationBuilder.RenameColumn(
                name: "FormData117",
                table: "SW_forms",
                newName: "formData_117");

            migrationBuilder.RenameColumn(
                name: "FormData116",
                table: "SW_forms",
                newName: "formData_116");

            migrationBuilder.RenameColumn(
                name: "FormData115",
                table: "SW_forms",
                newName: "formData_115");

            migrationBuilder.RenameColumn(
                name: "FormData114",
                table: "SW_forms",
                newName: "formData_114");

            migrationBuilder.RenameColumn(
                name: "FormData113",
                table: "SW_forms",
                newName: "formData_113");

            migrationBuilder.RenameColumn(
                name: "FormData112",
                table: "SW_forms",
                newName: "formData_112");

            migrationBuilder.RenameColumn(
                name: "FormData111",
                table: "SW_forms",
                newName: "formData_111");

            migrationBuilder.RenameColumn(
                name: "FormData110",
                table: "SW_forms",
                newName: "formData_110");

            migrationBuilder.RenameColumn(
                name: "FormData11",
                table: "SW_forms",
                newName: "formData_11");

            migrationBuilder.RenameColumn(
                name: "FormData109",
                table: "SW_forms",
                newName: "formData_109");

            migrationBuilder.RenameColumn(
                name: "FormData108",
                table: "SW_forms",
                newName: "formData_108");

            migrationBuilder.RenameColumn(
                name: "FormData107",
                table: "SW_forms",
                newName: "formData_107");

            migrationBuilder.RenameColumn(
                name: "FormData106",
                table: "SW_forms",
                newName: "formData_106");

            migrationBuilder.RenameColumn(
                name: "FormData105",
                table: "SW_forms",
                newName: "formData_105");

            migrationBuilder.RenameColumn(
                name: "FormData104",
                table: "SW_forms",
                newName: "formData_104");

            migrationBuilder.RenameColumn(
                name: "FormData103",
                table: "SW_forms",
                newName: "formData_103");

            migrationBuilder.RenameColumn(
                name: "FormData102",
                table: "SW_forms",
                newName: "formData_102");

            migrationBuilder.RenameColumn(
                name: "FormData101",
                table: "SW_forms",
                newName: "formData_101");

            migrationBuilder.RenameColumn(
                name: "FormData100",
                table: "SW_forms",
                newName: "formData_100");

            migrationBuilder.RenameColumn(
                name: "FormData10",
                table: "SW_forms",
                newName: "formData_10");

            migrationBuilder.RenameColumn(
                name: "FormData09",
                table: "SW_forms",
                newName: "formData_09");

            migrationBuilder.RenameColumn(
                name: "FormData08",
                table: "SW_forms",
                newName: "formData_08");

            migrationBuilder.RenameColumn(
                name: "FormData07",
                table: "SW_forms",
                newName: "formData_07");

            migrationBuilder.RenameColumn(
                name: "FormData06",
                table: "SW_forms",
                newName: "formData_06");

            migrationBuilder.RenameColumn(
                name: "FormData05",
                table: "SW_forms",
                newName: "formData_05");

            migrationBuilder.RenameColumn(
                name: "FormData04",
                table: "SW_forms",
                newName: "formData_04");

            migrationBuilder.RenameColumn(
                name: "FormData03",
                table: "SW_forms",
                newName: "formData_03");

            migrationBuilder.RenameColumn(
                name: "FormData02",
                table: "SW_forms",
                newName: "formData_02");

            migrationBuilder.RenameColumn(
                name: "FormData01",
                table: "SW_forms",
                newName: "formData_01");

            migrationBuilder.AlterColumn<string>(
                name: "media_03",
                table: "SW_identity",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "media_02",
                table: "SW_identity",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "media_01",
                table: "SW_identity",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "uuid",
                table: "SW_forms",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "form",
                table: "SW_forms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "desc",
                table: "SW_forms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SW_forms_tableNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SW_formsId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SW_forms_tableNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SW_formsSW_forms_tableNames",
                        column: x => x.SW_formsId,
                        principalTable: "SW_forms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FK_SW_formsSW_forms_tableNames",
                table: "SW_forms_tableNames",
                column: "SW_formsId");
        }
    }
}
