IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151335_DB Context More'
)
BEGIN
    CREATE TABLE [SW_identity] (
        [Id] int NOT NULL IDENTITY,
        [name] nvarchar(max) NOT NULL,
        [desc] nvarchar(max) NULL,
        [logo] nvarchar(max) NULL,
        [media_01] nvarchar(max) NOT NULL,
        [media_02] nvarchar(max) NOT NULL,
        [media_03] nvarchar(max) NOT NULL,
        [header] nvarchar(max) NULL,
        [signature] nvarchar(max) NULL,
        CONSTRAINT [PK_SW_identity] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151335_DB Context More'
)
BEGIN
    CREATE TABLE [SW_forms] (
        [Id] int NOT NULL IDENTITY,
        [uuid] int NULL,
        [name] nvarchar(max) NOT NULL,
        [desc] nvarchar(max) NOT NULL,
        [form] nvarchar(max) NOT NULL,
        [is_approval_01] tinyint NULL,
        [is_approval_02] tinyint NULL,
        [is_approval_03] tinyint NULL,
        [dateModified] datetime NOT NULL,
        [SW_identityId] int NOT NULL,
        [formData_01] nvarchar(max) NULL,
        [formData_02] nvarchar(max) NULL,
        [formData_03] nvarchar(max) NULL,
        [formData_04] nvarchar(max) NULL,
        [formData_05] nvarchar(max) NULL,
        [formData_06] nvarchar(max) NULL,
        [formData_07] nvarchar(max) NULL,
        [formData_08] nvarchar(max) NULL,
        [formData_09] nvarchar(max) NULL,
        [formData_10] nvarchar(max) NULL,
        [formData_11] nvarchar(max) NULL,
        [formData_12] nvarchar(max) NULL,
        [formData_13] nvarchar(max) NULL,
        [formData_14] nvarchar(max) NULL,
        [formData_15] nvarchar(max) NULL,
        [formData_16] nvarchar(max) NULL,
        [formData_17] nvarchar(max) NULL,
        [formData_18] nvarchar(max) NULL,
        [formData_19] nvarchar(max) NULL,
        [formData_20] nvarchar(max) NULL,
        [formData_21] nvarchar(max) NULL,
        [formData_22] nvarchar(max) NULL,
        [formData_23] nvarchar(max) NULL,
        [formData_24] nvarchar(max) NULL,
        [formData_25] nvarchar(max) NULL,
        [formData_26] nvarchar(max) NULL,
        [formData_27] nvarchar(max) NULL,
        [formData_28] nvarchar(max) NULL,
        [formData_29] nvarchar(max) NULL,
        [formData_30] nvarchar(max) NULL,
        [formData_31] nvarchar(max) NULL,
        [formData_32] nvarchar(max) NULL,
        [formData_33] nvarchar(max) NULL,
        [formData_34] nvarchar(max) NULL,
        [formData_35] nvarchar(max) NULL,
        [formData_36] nvarchar(max) NULL,
        [formData_37] nvarchar(max) NULL,
        [formData_38] nvarchar(max) NULL,
        [formData_39] nvarchar(max) NULL,
        [formData_40] nvarchar(max) NULL,
        [formData_41] nvarchar(max) NULL,
        [formData_42] nvarchar(max) NULL,
        [formData_43] nvarchar(max) NULL,
        [formData_44] nvarchar(max) NULL,
        [formData_45] nvarchar(max) NULL,
        [formData_46] nvarchar(max) NULL,
        [formData_47] nvarchar(max) NULL,
        [formData_48] nvarchar(max) NULL,
        [formData_49] nvarchar(max) NULL,
        [formData_50] nvarchar(max) NULL,
        [formData_51] nvarchar(max) NULL,
        [formData_52] nvarchar(max) NULL,
        [formData_53] nvarchar(max) NULL,
        [formData_54] nvarchar(max) NULL,
        [formData_55] nvarchar(max) NULL,
        [formData_56] nvarchar(max) NULL,
        [formData_57] nvarchar(max) NULL,
        [formData_58] nvarchar(max) NULL,
        [formData_59] nvarchar(max) NULL,
        [formData_60] nvarchar(max) NULL,
        [formData_61] nvarchar(max) NULL,
        [formData_62] nvarchar(max) NULL,
        [formData_63] nvarchar(max) NULL,
        [formData_64] nvarchar(max) NULL,
        [formData_65] nvarchar(max) NULL,
        [formData_66] nvarchar(max) NULL,
        [formData_67] nvarchar(max) NULL,
        [formData_68] nvarchar(max) NULL,
        [formData_69] nvarchar(max) NULL,
        [formData_70] nvarchar(max) NULL,
        [formData_71] nvarchar(max) NULL,
        [formData_72] nvarchar(max) NULL,
        [formData_73] nvarchar(max) NULL,
        [formData_74] nvarchar(max) NULL,
        [formData_75] nvarchar(max) NULL,
        [formData_76] nvarchar(max) NULL,
        [formData_77] nvarchar(max) NULL,
        [formData_78] nvarchar(max) NULL,
        [formData_79] nvarchar(max) NULL,
        [formData_80] nvarchar(max) NULL,
        [formData_81] nvarchar(max) NULL,
        [formData_82] nvarchar(max) NULL,
        [formData_83] nvarchar(max) NULL,
        [formData_84] nvarchar(max) NULL,
        [formData_85] nvarchar(max) NULL,
        [formData_86] nvarchar(max) NULL,
        [formData_87] nvarchar(max) NULL,
        [formData_88] nvarchar(max) NULL,
        [formData_89] nvarchar(max) NULL,
        [formData_90] nvarchar(max) NULL,
        [formData_91] nvarchar(max) NULL,
        [formData_92] nvarchar(max) NULL,
        [formData_93] nvarchar(max) NULL,
        [formData_94] nvarchar(max) NULL,
        [formData_95] nvarchar(max) NULL,
        [formData_96] nvarchar(max) NULL,
        [formData_97] nvarchar(max) NULL,
        [formData_98] nvarchar(max) NULL,
        [formData_99] nvarchar(max) NULL,
        [formData_100] nvarchar(max) NULL,
        [formData_101] nvarchar(max) NULL,
        [formData_102] nvarchar(max) NULL,
        [formData_103] nvarchar(max) NULL,
        [formData_104] nvarchar(max) NULL,
        [formData_105] nvarchar(max) NULL,
        [formData_106] nvarchar(max) NULL,
        [formData_107] nvarchar(max) NULL,
        [formData_108] nvarchar(max) NULL,
        [formData_109] nvarchar(max) NULL,
        [formData_110] nvarchar(max) NULL,
        [formData_111] nvarchar(max) NULL,
        [formData_112] nvarchar(max) NULL,
        [formData_113] nvarchar(max) NULL,
        [formData_114] nvarchar(max) NULL,
        [formData_115] nvarchar(max) NULL,
        [formData_116] nvarchar(max) NULL,
        [formData_117] nvarchar(max) NULL,
        [formData_118] nvarchar(max) NULL,
        [formData_119] nvarchar(max) NULL,
        [formData_120] nvarchar(max) NULL,
        [formData_121] nvarchar(max) NULL,
        [formData_122] nvarchar(max) NULL,
        [formData_123] nvarchar(max) NULL,
        [formData_124] nvarchar(max) NULL,
        [formData_125] nvarchar(max) NULL,
        [formData_126] nvarchar(max) NULL,
        [formData_127] nvarchar(max) NULL,
        [formData_128] nvarchar(max) NULL,
        [formData_129] nvarchar(max) NULL,
        [formData_130] nvarchar(max) NULL,
        [formData_131] nvarchar(max) NULL,
        [formData_132] nvarchar(max) NULL,
        [formData_133] nvarchar(max) NULL,
        [formData_134] nvarchar(max) NULL,
        [formData_135] nvarchar(max) NULL,
        [formData_136] nvarchar(max) NULL,
        [formData_137] nvarchar(max) NULL,
        [formData_138] nvarchar(max) NULL,
        [formData_139] nvarchar(max) NULL,
        [formData_140] nvarchar(max) NULL,
        [formData_141] nvarchar(max) NULL,
        [formData_142] nvarchar(max) NULL,
        [formData_143] nvarchar(max) NULL,
        [formData_144] nvarchar(max) NULL,
        [formData_145] nvarchar(max) NULL,
        [formData_146] nvarchar(max) NULL,
        [formData_147] nvarchar(max) NULL,
        [formData_148] nvarchar(max) NULL,
        [formData_149] nvarchar(max) NULL,
        [formData_150] nvarchar(max) NULL,
        [formData_151] nvarchar(max) NULL,
        [formData_152] nvarchar(max) NULL,
        [formData_153] nvarchar(max) NULL,
        [formData_154] nvarchar(max) NULL,
        [formData_155] nvarchar(max) NULL,
        [formData_156] nvarchar(max) NULL,
        [formData_157] nvarchar(max) NULL,
        [formData_158] nvarchar(max) NULL,
        [formData_159] nvarchar(max) NULL,
        [formData_160] nvarchar(max) NULL,
        [formData_161] nvarchar(max) NULL,
        [formData_162] nvarchar(max) NULL,
        [formData_163] nvarchar(max) NULL,
        [formData_164] nvarchar(max) NULL,
        [formData_165] nvarchar(max) NULL,
        [formData_166] nvarchar(max) NULL,
        [formData_167] nvarchar(max) NULL,
        [formData_168] nvarchar(max) NULL,
        [formData_169] nvarchar(max) NULL,
        [formData_170] nvarchar(max) NULL,
        [formData_171] nvarchar(max) NULL,
        [formData_172] nvarchar(max) NULL,
        [formData_173] nvarchar(max) NULL,
        [formData_174] nvarchar(max) NULL,
        [formData_175] nvarchar(max) NULL,
        [formData_176] nvarchar(max) NULL,
        [formData_177] nvarchar(max) NULL,
        [formData_178] nvarchar(max) NULL,
        [formData_179] nvarchar(max) NULL,
        [formData_180] nvarchar(max) NULL,
        [formData_181] nvarchar(max) NULL,
        [formData_182] nvarchar(max) NULL,
        [formData_183] nvarchar(max) NULL,
        [formData_184] nvarchar(max) NULL,
        [formData_185] nvarchar(max) NULL,
        [formData_186] nvarchar(max) NULL,
        [formData_187] nvarchar(max) NULL,
        [formData_188] nvarchar(max) NULL,
        [formData_189] nvarchar(max) NULL,
        [formData_190] nvarchar(max) NULL,
        [formData_191] nvarchar(max) NULL,
        [formData_192] nvarchar(max) NULL,
        [formData_193] nvarchar(max) NULL,
        [formData_194] nvarchar(max) NULL,
        [formData_195] nvarchar(max) NULL,
        [formData_196] nvarchar(max) NULL,
        [formData_197] nvarchar(max) NULL,
        [formData_198] nvarchar(max) NULL,
        [formData_199] nvarchar(max) NULL,
        [formData_200] nvarchar(max) NULL,
        [formData_201] nvarchar(max) NULL,
        [formData_202] nvarchar(max) NULL,
        [formData_203] nvarchar(max) NULL,
        [formData_204] nvarchar(max) NULL,
        [formData_205] nvarchar(max) NULL,
        [formData_206] nvarchar(max) NULL,
        [formData_207] nvarchar(max) NULL,
        [formData_208] nvarchar(max) NULL,
        [formData_209] nvarchar(max) NULL,
        [formData_210] nvarchar(max) NULL,
        [formData_211] nvarchar(max) NULL,
        [formData_212] nvarchar(max) NULL,
        [formData_213] nvarchar(max) NULL,
        [formData_214] nvarchar(max) NULL,
        [formData_215] nvarchar(max) NULL,
        [formData_216] nvarchar(max) NULL,
        [formData_217] nvarchar(max) NULL,
        [formData_218] nvarchar(max) NULL,
        [formData_219] nvarchar(max) NULL,
        [formData_220] nvarchar(max) NULL,
        [formData_221] nvarchar(max) NULL,
        [formData_222] nvarchar(max) NULL,
        [formData_223] nvarchar(max) NULL,
        [formData_224] nvarchar(max) NULL,
        [formData_225] nvarchar(max) NULL,
        [formData_226] nvarchar(max) NULL,
        [formData_227] nvarchar(max) NULL,
        [formData_228] nvarchar(max) NULL,
        [formData_229] nvarchar(max) NULL,
        [formData_230] nvarchar(max) NULL,
        [formData_231] nvarchar(max) NULL,
        [formData_232] nvarchar(max) NULL,
        [formData_233] nvarchar(max) NULL,
        [formData_234] nvarchar(max) NULL,
        [formData_235] nvarchar(max) NULL,
        [formData_236] nvarchar(max) NULL,
        [formData_237] nvarchar(max) NULL,
        [formData_238] nvarchar(max) NULL,
        [formData_239] nvarchar(max) NULL,
        [formData_240] nvarchar(max) NULL,
        [formData_241] nvarchar(max) NULL,
        [formData_242] nvarchar(max) NULL,
        [formData_243] nvarchar(max) NULL,
        [formData_244] nvarchar(max) NULL,
        [formData_245] nvarchar(max) NULL,
        [formData_246] nvarchar(max) NULL,
        [formData_247] nvarchar(max) NULL,
        [formData_248] nvarchar(max) NULL,
        [formData_249] nvarchar(max) NULL,
        [formData_250] nvarchar(max) NULL,
        CONSTRAINT [PK_SW_forms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SW_identitySW_forms] FOREIGN KEY ([SW_identityId]) REFERENCES [SW_identity] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151335_DB Context More'
)
BEGIN
    CREATE TABLE [SW_forms_tableNames] (
        [Id] int NOT NULL IDENTITY,
        [name] nvarchar(max) NOT NULL,
        [SW_formsId] int NOT NULL,
        CONSTRAINT [PK_SW_forms_tableNames] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SW_formsSW_forms_tableNames] FOREIGN KEY ([SW_formsId]) REFERENCES [SW_forms] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151335_DB Context More'
)
BEGIN
    CREATE INDEX [IX_FK_SW_identitySW_forms] ON [SW_forms] ([SW_identityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151335_DB Context More'
)
BEGIN
    CREATE INDEX [IX_FK_SW_formsSW_forms_tableNames] ON [SW_forms_tableNames] ([SW_formsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151335_DB Context More'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250814151335_DB Context More', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    DROP TABLE [SW_forms_tableNames];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[is_approval_03]', N'isApproval_03', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[is_approval_02]', N'isApproval_02', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[is_approval_01]', N'isApproval_01', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_99]', N'FormData99', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_98]', N'FormData98', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_97]', N'FormData97', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_96]', N'FormData96', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_95]', N'FormData95', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_94]', N'FormData94', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_93]', N'FormData93', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_92]', N'FormData92', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_91]', N'FormData91', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_90]', N'FormData90', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_89]', N'FormData89', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_88]', N'FormData88', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_87]', N'FormData87', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_86]', N'FormData86', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_85]', N'FormData85', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_84]', N'FormData84', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_83]', N'FormData83', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_82]', N'FormData82', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_81]', N'FormData81', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_80]', N'FormData80', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_79]', N'FormData79', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_78]', N'FormData78', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_77]', N'FormData77', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_76]', N'FormData76', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_75]', N'FormData75', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_74]', N'FormData74', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_73]', N'FormData73', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_72]', N'FormData72', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_71]', N'FormData71', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_70]', N'FormData70', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_69]', N'FormData69', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_68]', N'FormData68', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_67]', N'FormData67', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_66]', N'FormData66', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_65]', N'FormData65', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_64]', N'FormData64', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_63]', N'FormData63', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_62]', N'FormData62', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_61]', N'FormData61', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_60]', N'FormData60', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_59]', N'FormData59', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_58]', N'FormData58', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_57]', N'FormData57', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_56]', N'FormData56', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_55]', N'FormData55', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_54]', N'FormData54', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_53]', N'FormData53', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_52]', N'FormData52', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_51]', N'FormData51', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_50]', N'FormData50', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_49]', N'FormData49', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_48]', N'FormData48', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_47]', N'FormData47', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_46]', N'FormData46', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_45]', N'FormData45', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_44]', N'FormData44', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_43]', N'FormData43', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_42]', N'FormData42', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_41]', N'FormData41', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_40]', N'FormData40', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_39]', N'FormData39', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_38]', N'FormData38', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_37]', N'FormData37', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_36]', N'FormData36', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_35]', N'FormData35', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_34]', N'FormData34', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_33]', N'FormData33', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_32]', N'FormData32', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_31]', N'FormData31', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_30]', N'FormData30', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_29]', N'FormData29', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_28]', N'FormData28', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_27]', N'FormData27', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_26]', N'FormData26', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_250]', N'FormData250', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_25]', N'FormData25', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_249]', N'FormData249', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_248]', N'FormData248', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_247]', N'FormData247', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_246]', N'FormData246', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_245]', N'FormData245', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_244]', N'FormData244', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_243]', N'FormData243', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_242]', N'FormData242', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_241]', N'FormData241', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_240]', N'FormData240', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_24]', N'FormData24', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_239]', N'FormData239', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_238]', N'FormData238', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_237]', N'FormData237', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_236]', N'FormData236', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_235]', N'FormData235', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_234]', N'FormData234', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_233]', N'FormData233', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_232]', N'FormData232', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_231]', N'FormData231', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_230]', N'FormData230', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_23]', N'FormData23', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_229]', N'FormData229', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_228]', N'FormData228', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_227]', N'FormData227', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_226]', N'FormData226', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_225]', N'FormData225', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_224]', N'FormData224', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_223]', N'FormData223', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_222]', N'FormData222', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_221]', N'FormData221', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_220]', N'FormData220', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_22]', N'FormData22', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_219]', N'FormData219', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_218]', N'FormData218', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_217]', N'FormData217', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_216]', N'FormData216', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_215]', N'FormData215', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_214]', N'FormData214', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_213]', N'FormData213', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_212]', N'FormData212', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_211]', N'FormData211', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_210]', N'FormData210', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_21]', N'FormData21', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_209]', N'FormData209', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_208]', N'FormData208', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_207]', N'FormData207', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_206]', N'FormData206', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_205]', N'FormData205', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_204]', N'FormData204', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_203]', N'FormData203', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_202]', N'FormData202', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_201]', N'FormData201', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_200]', N'FormData200', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_20]', N'FormData20', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_199]', N'FormData199', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_198]', N'FormData198', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_197]', N'FormData197', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_196]', N'FormData196', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_195]', N'FormData195', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_194]', N'FormData194', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_193]', N'FormData193', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_192]', N'FormData192', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_191]', N'FormData191', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_190]', N'FormData190', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_19]', N'FormData19', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_189]', N'FormData189', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_188]', N'FormData188', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_187]', N'FormData187', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_186]', N'FormData186', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_185]', N'FormData185', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_184]', N'FormData184', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_183]', N'FormData183', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_182]', N'FormData182', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_181]', N'FormData181', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_180]', N'FormData180', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_18]', N'FormData18', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_179]', N'FormData179', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_178]', N'FormData178', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_177]', N'FormData177', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_176]', N'FormData176', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_175]', N'FormData175', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_174]', N'FormData174', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_173]', N'FormData173', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_172]', N'FormData172', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_171]', N'FormData171', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_170]', N'FormData170', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_17]', N'FormData17', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_169]', N'FormData169', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_168]', N'FormData168', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_167]', N'FormData167', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_166]', N'FormData166', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_165]', N'FormData165', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_164]', N'FormData164', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_163]', N'FormData163', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_162]', N'FormData162', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_161]', N'FormData161', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_160]', N'FormData160', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_16]', N'FormData16', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_159]', N'FormData159', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_158]', N'FormData158', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_157]', N'FormData157', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_156]', N'FormData156', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_155]', N'FormData155', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_154]', N'FormData154', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_153]', N'FormData153', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_152]', N'FormData152', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_151]', N'FormData151', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_150]', N'FormData150', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_15]', N'FormData15', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_149]', N'FormData149', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_148]', N'FormData148', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_147]', N'FormData147', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_146]', N'FormData146', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_145]', N'FormData145', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_144]', N'FormData144', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_143]', N'FormData143', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_142]', N'FormData142', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_141]', N'FormData141', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_140]', N'FormData140', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_14]', N'FormData14', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_139]', N'FormData139', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_138]', N'FormData138', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_137]', N'FormData137', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_136]', N'FormData136', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_135]', N'FormData135', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_134]', N'FormData134', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_133]', N'FormData133', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_132]', N'FormData132', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_131]', N'FormData131', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_130]', N'FormData130', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_13]', N'FormData13', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_129]', N'FormData129', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_128]', N'FormData128', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_127]', N'FormData127', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_126]', N'FormData126', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_125]', N'FormData125', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_124]', N'FormData124', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_123]', N'FormData123', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_122]', N'FormData122', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_121]', N'FormData121', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_120]', N'FormData120', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_12]', N'FormData12', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_119]', N'FormData119', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_118]', N'FormData118', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_117]', N'FormData117', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_116]', N'FormData116', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_115]', N'FormData115', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_114]', N'FormData114', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_113]', N'FormData113', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_112]', N'FormData112', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_111]', N'FormData111', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_110]', N'FormData110', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_11]', N'FormData11', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_109]', N'FormData109', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_108]', N'FormData108', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_107]', N'FormData107', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_106]', N'FormData106', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_105]', N'FormData105', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_104]', N'FormData104', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_103]', N'FormData103', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_102]', N'FormData102', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_101]', N'FormData101', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_100]', N'FormData100', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_10]', N'FormData10', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_09]', N'FormData09', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_08]', N'FormData08', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_07]', N'FormData07', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_06]', N'FormData06', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_05]', N'FormData05', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_04]', N'FormData04', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_03]', N'FormData03', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_02]', N'FormData02', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    EXEC sp_rename N'[SW_forms].[formData_01]', N'FormData01', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SW_identity]') AND [c].[name] = N'media_03');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [SW_identity] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [SW_identity] ALTER COLUMN [media_03] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SW_identity]') AND [c].[name] = N'media_02');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [SW_identity] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [SW_identity] ALTER COLUMN [media_02] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SW_identity]') AND [c].[name] = N'media_01');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [SW_identity] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [SW_identity] ALTER COLUMN [media_01] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SW_forms]') AND [c].[name] = N'uuid');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [SW_forms] DROP CONSTRAINT [' + @var3 + '];');
    EXEC(N'UPDATE [SW_forms] SET [uuid] = 0 WHERE [uuid] IS NULL');
    ALTER TABLE [SW_forms] ALTER COLUMN [uuid] int NOT NULL;
    ALTER TABLE [SW_forms] ADD DEFAULT 0 FOR [uuid];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SW_forms]') AND [c].[name] = N'form');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [SW_forms] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [SW_forms] ALTER COLUMN [form] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SW_forms]') AND [c].[name] = N'desc');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [SW_forms] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [SW_forms] ALTER COLUMN [desc] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151731_DB Context More again'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250814151731_DB Context More again', N'9.0.8');
END;

COMMIT;
GO