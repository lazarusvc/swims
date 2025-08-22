
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 08/22/2025 11:29:39
-- Generated from EDMX file: C:\Users\lazarusa\Documents\Git Projects\SWIMS\MAIN__2.0\SWIMS\Model_Designer\Model.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [SWIMS_DB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

--IF OBJECT_ID(N'[sp].[FK_stored_process_params_stored_processes_StoredProcessId]', 'F') IS NOT NULL
--    ALTER TABLE [sp].[stored_process_params] DROP CONSTRAINT [FK_stored_process_params_stored_processes_StoredProcessId];
--GO
--IF OBJECT_ID(N'[dbo].[FK_SW_formsSW_formTableData]', 'F') IS NOT NULL
--    ALTER TABLE [dbo].[SW_formTableData] DROP CONSTRAINT [FK_SW_formsSW_formTableData];
--GO
--IF OBJECT_ID(N'[dbo].[FK_SW_formsSW_formTableData_Types]', 'F') IS NOT NULL
--    ALTER TABLE [dbo].[SW_formTableData_Types] DROP CONSTRAINT [FK_SW_formsSW_formTableData_Types];
--GO
--IF OBJECT_ID(N'[dbo].[FK_SW_formsSW_formTableName]', 'F') IS NOT NULL
--    ALTER TABLE [dbo].[SW_formTableName] DROP CONSTRAINT [FK_SW_formsSW_formTableName];
--GO
--IF OBJECT_ID(N'[dbo].[FK_SW_identitySW_forms]', 'F') IS NOT NULL
--    ALTER TABLE [dbo].[SW_forms] DROP CONSTRAINT [FK_SW_identitySW_forms];
--GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

--IF OBJECT_ID(N'[dbo].[SW_forms]', 'U') IS NOT NULL
--    DROP TABLE [dbo].[SW_forms];
--GO
--IF OBJECT_ID(N'[dbo].[SW_formTableData]', 'U') IS NOT NULL
--    DROP TABLE [dbo].[SW_formTableData];
--GO
--IF OBJECT_ID(N'[dbo].[SW_formTableData_Types]', 'U') IS NOT NULL
--    DROP TABLE [dbo].[SW_formTableData_Types];
--GO
--IF OBJECT_ID(N'[dbo].[SW_formTableName]', 'U') IS NOT NULL
--    DROP TABLE [dbo].[SW_formTableName];
--GO
--IF OBJECT_ID(N'[dbo].[SW_identity]', 'U') IS NOT NULL
--    DROP TABLE [dbo].[SW_identity];
--GO
--IF OBJECT_ID(N'[dbo].[SW_roles]', 'U') IS NOT NULL
--    DROP TABLE [dbo].[SW_roles];
--GO
--IF OBJECT_ID(N'[dbo].[SW_users]', 'U') IS NOT NULL
--    DROP TABLE [dbo].[SW_users];
--GO
--IF OBJECT_ID(N'[sp].[stored_process_params]', 'U') IS NOT NULL
--    DROP TABLE [sp].[stored_process_params];
--GO
--IF OBJECT_ID(N'[sp].[stored_processes]', 'U') IS NOT NULL
--    DROP TABLE [sp].[stored_processes];
--GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'SW_formTableName'
--CREATE TABLE [dbo].[SW_formTableName] (
--    [Id] int IDENTITY(1,1) NOT NULL,
--    [name] nvarchar(max)  NULL,
--    [field] nvarchar(max)  NULL,
--    [SW_formsId] int  NOT NULL
--);
--GO

---- Creating table 'SW_identity'
--CREATE TABLE [dbo].[SW_identity] (
--    [Id] int IDENTITY(1,1) NOT NULL,
--    [name] nvarchar(max)  NOT NULL,
--    [desc] nvarchar(max)  NULL,
--    [logo] nvarchar(max)  NULL,
--    [media_01] nvarchar(max)  NULL,
--    [media_02] nvarchar(max)  NULL,
--    [media_03] nvarchar(max)  NULL,
--    [header] nvarchar(max)  NULL,
--    [signature] nvarchar(max)  NULL
--);
--GO

---- Creating table 'SW_roles'
--CREATE TABLE [dbo].[SW_roles] (
--    [Id] int IDENTITY(1,1) NOT NULL,
--    [name] nvarchar(256)  NULL,
--    [NormalizedName] nvarchar(256)  NULL,
--    [ConcurrencyStamp] nvarchar(max)  NULL
--);
--GO

---- Creating table 'SW_users'
--CREATE TABLE [dbo].[SW_users] (
--    [Id] int IDENTITY(1,1) NOT NULL,
--    [fullName] nvarchar(max)  NOT NULL,
--    [userName] nvarchar(max)  NOT NULL,
--    [email] nvarchar(max)  NOT NULL,
--    [password] nvarchar(max)  NOT NULL,
--    [SW_rolesId] int  NOT NULL,
--    [FirstName] nvarchar(max)  NULL,
--    [LastName] nvarchar(max)  NULL,
--    [UserName] nvarchar(256)  NULL,
--    [NormalizedUserName] nvarchar(256)  NULL,
--    [Email] nvarchar(256)  NULL,
--    [NormalizedEmail] nvarchar(256)  NULL,
--    [EmailConfirmed] bit  NOT NULL,
--    [PasswordHash] nvarchar(max)  NULL,
--    [SecurityStamp] nvarchar(max)  NULL,
--    [ConcurrencyStamp] nvarchar(max)  NULL,
--    [PhoneNumber] nvarchar(max)  NULL,
--    [PhoneNumberConfirmed] bit  NOT NULL,
--    [TwoFactorEnabled] bit  NOT NULL,
--    [LockoutEnd] datetimeoffset  NULL,
--    [LockoutEnabled] bit  NOT NULL,
--    [AccessFailedCount] int  NOT NULL
--);
--GO

---- Creating table 'SW_formTableData_Types'
--CREATE TABLE [dbo].[SW_formTableData_Types] (
--    [Id] int IDENTITY(1,1) NOT NULL,
--    [type] nvarchar(max)  NULL,
--    [field] nvarchar(max)  NULL,
--    [SW_formsId] int  NOT NULL
--);
--GO

---- Creating table 'SW_forms'
--CREATE TABLE [dbo].[SW_forms] (
--    [Id] int IDENTITY(1,1) NOT NULL,
--    [uuid] nvarchar(max)  NULL,
--    [name] nvarchar(max)  NOT NULL,
--    [desc] nvarchar(max)  NULL,
--    [form] nvarchar(max)  NULL,
--    [dateModified] datetime  NOT NULL,
--    [SW_identityId] int  NOT NULL
--);
--GO

---- Creating table 'SW_formTableData'
--CREATE TABLE [dbo].[SW_formTableData] (
--    [Id] int IDENTITY(1,1) NOT NULL,
--    [FormData01] nvarchar(max)  NULL,
--    [FormData02] nvarchar(max)  NULL,
--    [FormData03] nvarchar(max)  NULL,
--    [FormData04] nvarchar(max)  NULL,
--    [FormData05] nvarchar(max)  NULL,
--    [FormData06] nvarchar(max)  NULL,
--    [FormData07] nvarchar(max)  NULL,
--    [FormData08] nvarchar(max)  NULL,
--    [FormData09] nvarchar(max)  NULL,
--    [FormData10] nvarchar(max)  NULL,
--    [FormData11] nvarchar(max)  NULL,
--    [FormData12] nvarchar(max)  NULL,
--    [FormData13] nvarchar(max)  NULL,
--    [FormData14] nvarchar(max)  NULL,
--    [FormData15] nvarchar(max)  NULL,
--    [FormData16] nvarchar(max)  NULL,
--    [FormData17] nvarchar(max)  NULL,
--    [FormData18] nvarchar(max)  NULL,
--    [FormData19] nvarchar(max)  NULL,
--    [FormData20] nvarchar(max)  NULL,
--    [FormData21] nvarchar(max)  NULL,
--    [FormData22] nvarchar(max)  NULL,
--    [FormData23] nvarchar(max)  NULL,
--    [FormData24] nvarchar(max)  NULL,
--    [FormData25] nvarchar(max)  NULL,
--    [FormData26] nvarchar(max)  NULL,
--    [FormData27] nvarchar(max)  NULL,
--    [FormData28] nvarchar(max)  NULL,
--    [FormData29] nvarchar(max)  NULL,
--    [FormData30] nvarchar(max)  NULL,
--    [FormData31] nvarchar(max)  NULL,
--    [FormData32] nvarchar(max)  NULL,
--    [FormData33] nvarchar(max)  NULL,
--    [FormData34] nvarchar(max)  NULL,
--    [FormData35] nvarchar(max)  NULL,
--    [FormData36] nvarchar(max)  NULL,
--    [FormData37] nvarchar(max)  NULL,
--    [FormData38] nvarchar(max)  NULL,
--    [FormData39] nvarchar(max)  NULL,
--    [FormData40] nvarchar(max)  NULL,
--    [FormData41] nvarchar(max)  NULL,
--    [FormData42] nvarchar(max)  NULL,
--    [FormData43] nvarchar(max)  NULL,
--    [FormData44] nvarchar(max)  NULL,
--    [FormData45] nvarchar(max)  NULL,
--    [FormData46] nvarchar(max)  NULL,
--    [FormData47] nvarchar(max)  NULL,
--    [FormData48] nvarchar(max)  NULL,
--    [FormData49] nvarchar(max)  NULL,
--    [FormData50] nvarchar(max)  NULL,
--    [FormData51] nvarchar(max)  NULL,
--    [FormData52] nvarchar(max)  NULL,
--    [FormData53] nvarchar(max)  NULL,
--    [FormData54] nvarchar(max)  NULL,
--    [FormData55] nvarchar(max)  NULL,
--    [FormData56] nvarchar(max)  NULL,
--    [FormData57] nvarchar(max)  NULL,
--    [FormData58] nvarchar(max)  NULL,
--    [FormData59] nvarchar(max)  NULL,
--    [FormData60] nvarchar(max)  NULL,
--    [FormData61] nvarchar(max)  NULL,
--    [FormData62] nvarchar(max)  NULL,
--    [FormData63] nvarchar(max)  NULL,
--    [FormData64] nvarchar(max)  NULL,
--    [FormData65] nvarchar(max)  NULL,
--    [FormData66] nvarchar(max)  NULL,
--    [FormData67] nvarchar(max)  NULL,
--    [FormData68] nvarchar(max)  NULL,
--    [FormData69] nvarchar(max)  NULL,
--    [FormData70] nvarchar(max)  NULL,
--    [FormData71] nvarchar(max)  NULL,
--    [FormData72] nvarchar(max)  NULL,
--    [FormData73] nvarchar(max)  NULL,
--    [FormData74] nvarchar(max)  NULL,
--    [FormData75] nvarchar(max)  NULL,
--    [FormData76] nvarchar(max)  NULL,
--    [FormData77] nvarchar(max)  NULL,
--    [FormData78] nvarchar(max)  NULL,
--    [FormData79] nvarchar(max)  NULL,
--    [FormData80] nvarchar(max)  NULL,
--    [FormData81] nvarchar(max)  NULL,
--    [FormData82] nvarchar(max)  NULL,
--    [FormData83] nvarchar(max)  NULL,
--    [FormData84] nvarchar(max)  NULL,
--    [FormData85] nvarchar(max)  NULL,
--    [FormData86] nvarchar(max)  NULL,
--    [FormData87] nvarchar(max)  NULL,
--    [FormData88] nvarchar(max)  NULL,
--    [FormData89] nvarchar(max)  NULL,
--    [FormData90] nvarchar(max)  NULL,
--    [FormData91] nvarchar(max)  NULL,
--    [FormData92] nvarchar(max)  NULL,
--    [FormData93] nvarchar(max)  NULL,
--    [FormData94] nvarchar(max)  NULL,
--    [FormData95] nvarchar(max)  NULL,
--    [FormData96] nvarchar(max)  NULL,
--    [FormData97] nvarchar(max)  NULL,
--    [FormData98] nvarchar(max)  NULL,
--    [FormData99] nvarchar(max)  NULL,
--    [FormData100] nvarchar(max)  NULL,
--    [FormData101] nvarchar(max)  NULL,
--    [FormData102] nvarchar(max)  NULL,
--    [FormData103] nvarchar(max)  NULL,
--    [FormData104] nvarchar(max)  NULL,
--    [FormData105] nvarchar(max)  NULL,
--    [FormData106] nvarchar(max)  NULL,
--    [FormData107] nvarchar(max)  NULL,
--    [FormData108] nvarchar(max)  NULL,
--    [FormData109] nvarchar(max)  NULL,
--    [FormData110] nvarchar(max)  NULL,
--    [FormData111] nvarchar(max)  NULL,
--    [FormData112] nvarchar(max)  NULL,
--    [FormData113] nvarchar(max)  NULL,
--    [FormData114] nvarchar(max)  NULL,
--    [FormData115] nvarchar(max)  NULL,
--    [FormData116] nvarchar(max)  NULL,
--    [FormData117] nvarchar(max)  NULL,
--    [FormData118] nvarchar(max)  NULL,
--    [FormData119] nvarchar(max)  NULL,
--    [FormData120] nvarchar(max)  NULL,
--    [FormData121] nvarchar(max)  NULL,
--    [FormData122] nvarchar(max)  NULL,
--    [FormData123] nvarchar(max)  NULL,
--    [FormData124] nvarchar(max)  NULL,
--    [FormData125] nvarchar(max)  NULL,
--    [FormData126] nvarchar(max)  NULL,
--    [FormData127] nvarchar(max)  NULL,
--    [FormData128] nvarchar(max)  NULL,
--    [FormData129] nvarchar(max)  NULL,
--    [FormData130] nvarchar(max)  NULL,
--    [FormData131] nvarchar(max)  NULL,
--    [FormData132] nvarchar(max)  NULL,
--    [FormData133] nvarchar(max)  NULL,
--    [FormData134] nvarchar(max)  NULL,
--    [FormData135] nvarchar(max)  NULL,
--    [FormData136] nvarchar(max)  NULL,
--    [FormData137] nvarchar(max)  NULL,
--    [FormData138] nvarchar(max)  NULL,
--    [FormData139] nvarchar(max)  NULL,
--    [FormData140] nvarchar(max)  NULL,
--    [FormData141] nvarchar(max)  NULL,
--    [FormData142] nvarchar(max)  NULL,
--    [FormData143] nvarchar(max)  NULL,
--    [FormData144] nvarchar(max)  NULL,
--    [FormData145] nvarchar(max)  NULL,
--    [FormData146] nvarchar(max)  NULL,
--    [FormData147] nvarchar(max)  NULL,
--    [FormData148] nvarchar(max)  NULL,
--    [FormData149] nvarchar(max)  NULL,
--    [FormData150] nvarchar(max)  NULL,
--    [FormData151] nvarchar(max)  NULL,
--    [FormData152] nvarchar(max)  NULL,
--    [FormData153] nvarchar(max)  NULL,
--    [FormData154] nvarchar(max)  NULL,
--    [FormData155] nvarchar(max)  NULL,
--    [FormData156] nvarchar(max)  NULL,
--    [FormData157] nvarchar(max)  NULL,
--    [FormData158] nvarchar(max)  NULL,
--    [FormData159] nvarchar(max)  NULL,
--    [FormData160] nvarchar(max)  NULL,
--    [FormData161] nvarchar(max)  NULL,
--    [FormData162] nvarchar(max)  NULL,
--    [FormData163] nvarchar(max)  NULL,
--    [FormData164] nvarchar(max)  NULL,
--    [FormData165] nvarchar(max)  NULL,
--    [FormData166] nvarchar(max)  NULL,
--    [FormData167] nvarchar(max)  NULL,
--    [FormData168] nvarchar(max)  NULL,
--    [FormData169] nvarchar(max)  NULL,
--    [FormData170] nvarchar(max)  NULL,
--    [FormData171] nvarchar(max)  NULL,
--    [FormData172] nvarchar(max)  NULL,
--    [FormData173] nvarchar(max)  NULL,
--    [FormData174] nvarchar(max)  NULL,
--    [FormData175] nvarchar(max)  NULL,
--    [FormData176] nvarchar(max)  NULL,
--    [FormData177] nvarchar(max)  NULL,
--    [FormData178] nvarchar(max)  NULL,
--    [FormData179] nvarchar(max)  NULL,
--    [FormData180] nvarchar(max)  NULL,
--    [FormData181] nvarchar(max)  NULL,
--    [FormData182] nvarchar(max)  NULL,
--    [FormData183] nvarchar(max)  NULL,
--    [FormData184] nvarchar(max)  NULL,
--    [FormData185] nvarchar(max)  NULL,
--    [FormData186] nvarchar(max)  NULL,
--    [FormData187] nvarchar(max)  NULL,
--    [FormData188] nvarchar(max)  NULL,
--    [FormData189] nvarchar(max)  NULL,
--    [FormData190] nvarchar(max)  NULL,
--    [FormData191] nvarchar(max)  NULL,
--    [FormData192] nvarchar(max)  NULL,
--    [FormData193] nvarchar(max)  NULL,
--    [FormData194] nvarchar(max)  NULL,
--    [FormData195] nvarchar(max)  NULL,
--    [FormData196] nvarchar(max)  NULL,
--    [FormData197] nvarchar(max)  NULL,
--    [FormData198] nvarchar(max)  NULL,
--    [FormData199] nvarchar(max)  NULL,
--    [FormData200] nvarchar(max)  NULL,
--    [FormData201] nvarchar(max)  NULL,
--    [FormData202] nvarchar(max)  NULL,
--    [FormData203] nvarchar(max)  NULL,
--    [FormData204] nvarchar(max)  NULL,
--    [FormData205] nvarchar(max)  NULL,
--    [FormData206] nvarchar(max)  NULL,
--    [FormData207] nvarchar(max)  NULL,
--    [FormData208] nvarchar(max)  NULL,
--    [FormData209] nvarchar(max)  NULL,
--    [FormData210] nvarchar(max)  NULL,
--    [FormData211] nvarchar(max)  NULL,
--    [FormData212] nvarchar(max)  NULL,
--    [FormData213] nvarchar(max)  NULL,
--    [FormData214] nvarchar(max)  NULL,
--    [FormData215] nvarchar(max)  NULL,
--    [FormData216] nvarchar(max)  NULL,
--    [FormData217] nvarchar(max)  NULL,
--    [FormData218] nvarchar(max)  NULL,
--    [FormData219] nvarchar(max)  NULL,
--    [FormData220] nvarchar(max)  NULL,
--    [FormData221] nvarchar(max)  NULL,
--    [FormData222] nvarchar(max)  NULL,
--    [FormData223] nvarchar(max)  NULL,
--    [FormData224] nvarchar(max)  NULL,
--    [FormData225] nvarchar(max)  NULL,
--    [FormData226] nvarchar(max)  NULL,
--    [FormData227] nvarchar(max)  NULL,
--    [FormData228] nvarchar(max)  NULL,
--    [FormData229] nvarchar(max)  NULL,
--    [FormData230] nvarchar(max)  NULL,
--    [FormData231] nvarchar(max)  NULL,
--    [FormData232] nvarchar(max)  NULL,
--    [FormData233] nvarchar(max)  NULL,
--    [FormData234] nvarchar(max)  NULL,
--    [FormData235] nvarchar(max)  NULL,
--    [FormData236] nvarchar(max)  NULL,
--    [FormData237] nvarchar(max)  NULL,
--    [FormData238] nvarchar(max)  NULL,
--    [FormData239] nvarchar(max)  NULL,
--    [FormData240] nvarchar(max)  NULL,
--    [FormData241] nvarchar(max)  NULL,
--    [FormData242] nvarchar(max)  NULL,
--    [FormData243] nvarchar(max)  NULL,
--    [FormData244] nvarchar(max)  NULL,
--    [FormData245] nvarchar(max)  NULL,
--    [FormData246] nvarchar(max)  NULL,
--    [FormData247] nvarchar(max)  NULL,
--    [FormData248] nvarchar(max)  NULL,
--    [FormData249] nvarchar(max)  NULL,
--    [FormData250] nvarchar(max)  NULL,
--    [SW_formsId] int  NOT NULL,
--    [isApproval_01] tinyint  NULL,
--    [isApproval_02] tinyint  NULL,
--    [isApproval_03] tinyint  NULL
--);
--GO

---- Creating table 'stored_process_params'
--CREATE TABLE [dbo].[stored_process_params] (
--    [Id] int IDENTITY(1,1) NOT NULL,
--    [StoredProcessId] int  NOT NULL,
--    [Key] nvarchar(128)  NOT NULL,
--    [Value] nvarchar(max)  NULL,
--    [DataType] nvarchar(64)  NOT NULL
--);
--GO

---- Creating table 'stored_processes'
--CREATE TABLE [dbo].[stored_processes] (
--    [Id] int IDENTITY(1,1) NOT NULL,
--    [Name] nvarchar(256)  NOT NULL,
--    [Description] nvarchar(1024)  NOT NULL,
--    [ConnectionKey] nvarchar(128)  NULL,
--    [DataSource] nvarchar(256)  NULL,
--    [Database] nvarchar(256)  NULL,
--    [DbUserEncrypted] nvarchar(512)  NULL,
--    [DbPasswordEncrypted] nvarchar(1024)  NULL
--);
--GO

-- Creating table 'SW_formProcesses'
CREATE TABLE [dbo].[SW_formProcesses] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [url] nvarchar(max)  NOT NULL,
    [SW_formsId] int  NOT NULL
);
GO

-- Creating table 'SW_formReport'
CREATE TABLE [dbo].[SW_formReport] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [url] nvarchar(max)  NOT NULL,
    [SW_formsId] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'SW_formTableName'
--ALTER TABLE [dbo].[SW_formTableName]
--ADD CONSTRAINT [PK_SW_formTableName]
--    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

---- Creating primary key on [Id] in table 'SW_identity'
--ALTER TABLE [dbo].[SW_identity]
--ADD CONSTRAINT [PK_SW_identity]
--    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

---- Creating primary key on [Id] in table 'SW_roles'
--ALTER TABLE [dbo].[SW_roles]
--ADD CONSTRAINT [PK_SW_roles]
--    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

---- Creating primary key on [Id] in table 'SW_users'
--ALTER TABLE [dbo].[SW_users]
--ADD CONSTRAINT [PK_SW_users]
--    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

---- Creating primary key on [Id] in table 'SW_formTableData_Types'
--ALTER TABLE [dbo].[SW_formTableData_Types]
--ADD CONSTRAINT [PK_SW_formTableData_Types]
--    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

---- Creating primary key on [Id] in table 'SW_forms'
--ALTER TABLE [dbo].[SW_forms]
--ADD CONSTRAINT [PK_SW_forms]
--    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

---- Creating primary key on [Id] in table 'SW_formTableData'
--ALTER TABLE [dbo].[SW_formTableData]
--ADD CONSTRAINT [PK_SW_formTableData]
--    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

---- Creating primary key on [Id] in table 'stored_process_params'
--ALTER TABLE [dbo].[stored_process_params]
--ADD CONSTRAINT [PK_stored_process_params]
--    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

---- Creating primary key on [Id] in table 'stored_processes'
--ALTER TABLE [dbo].[stored_processes]
--ADD CONSTRAINT [PK_stored_processes]
--    PRIMARY KEY CLUSTERED ([Id] ASC);
--GO

-- Creating primary key on [Id] in table 'SW_formProcesses'
ALTER TABLE [dbo].[SW_formProcesses]
ADD CONSTRAINT [PK_SW_formProcesses]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SW_formReport'
ALTER TABLE [dbo].[SW_formReport]
ADD CONSTRAINT [PK_SW_formReport]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [SW_rolesId] in table 'SW_users'
ALTER TABLE [dbo].[SW_users]
ADD CONSTRAINT [FK_SW_rolesSW_users]
    FOREIGN KEY ([SW_rolesId])
    REFERENCES [dbo].[SW_roles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SW_rolesSW_users'
CREATE INDEX [IX_FK_SW_rolesSW_users]
ON [dbo].[SW_users]
    ([SW_rolesId]);
GO

-- Creating foreign key on [SW_formsId] in table 'SW_formTableData'
ALTER TABLE [dbo].[SW_formTableData]
ADD CONSTRAINT [FK_SW_formsSW_formTableData]
    FOREIGN KEY ([SW_formsId])
    REFERENCES [dbo].[SW_forms]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SW_formsSW_formTableData'
CREATE INDEX [IX_FK_SW_formsSW_formTableData]
ON [dbo].[SW_formTableData]
    ([SW_formsId]);
GO

-- Creating foreign key on [SW_formsId] in table 'SW_formTableData_Types'
ALTER TABLE [dbo].[SW_formTableData_Types]
ADD CONSTRAINT [FK_SW_formsSW_formTableData_Types]
    FOREIGN KEY ([SW_formsId])
    REFERENCES [dbo].[SW_forms]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SW_formsSW_formTableData_Types'
CREATE INDEX [IX_FK_SW_formsSW_formTableData_Types]
ON [dbo].[SW_formTableData_Types]
    ([SW_formsId]);
GO

-- Creating foreign key on [SW_formsId] in table 'SW_formTableName'
ALTER TABLE [dbo].[SW_formTableName]
ADD CONSTRAINT [FK_SW_formsSW_formTableName]
    FOREIGN KEY ([SW_formsId])
    REFERENCES [dbo].[SW_forms]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SW_formsSW_formTableName'
CREATE INDEX [IX_FK_SW_formsSW_formTableName]
ON [dbo].[SW_formTableName]
    ([SW_formsId]);
GO

-- Creating foreign key on [SW_identityId] in table 'SW_forms'
ALTER TABLE [dbo].[SW_forms]
ADD CONSTRAINT [FK_SW_identitySW_forms]
    FOREIGN KEY ([SW_identityId])
    REFERENCES [dbo].[SW_identity]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SW_identitySW_forms'
CREATE INDEX [IX_FK_SW_identitySW_forms]
ON [dbo].[SW_forms]
    ([SW_identityId]);
GO

-- Creating foreign key on [StoredProcessId] in table 'stored_process_params'
ALTER TABLE [dbo].[stored_process_params]
ADD CONSTRAINT [FK_stored_process_params_stored_processes_StoredProcessId]
    FOREIGN KEY ([StoredProcessId])
    REFERENCES [dbo].[stored_processes]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_stored_process_params_stored_processes_StoredProcessId'
CREATE INDEX [IX_FK_stored_process_params_stored_processes_StoredProcessId]
ON [dbo].[stored_process_params]
    ([StoredProcessId]);
GO

-- Creating foreign key on [SW_formsId] in table 'SW_formProcesses'
ALTER TABLE [dbo].[SW_formProcesses]
ADD CONSTRAINT [FK_SW_formsSW_formProcesses]
    FOREIGN KEY ([SW_formsId])
    REFERENCES [dbo].[SW_forms]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SW_formsSW_formProcesses'
CREATE INDEX [IX_FK_SW_formsSW_formProcesses]
ON [dbo].[SW_formProcesses]
    ([SW_formsId]);
GO

-- Creating foreign key on [SW_formsId] in table 'SW_formReport'
ALTER TABLE [dbo].[SW_formReport]
ADD CONSTRAINT [FK_SW_formsSW_formReport]
    FOREIGN KEY ([SW_formsId])
    REFERENCES [dbo].[SW_forms]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SW_formsSW_formReport'
CREATE INDEX [IX_FK_SW_formsSW_formReport]
ON [dbo].[SW_formReport]
    ([SW_formsId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------