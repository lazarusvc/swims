
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 06/23/2025 12:55:21
-- Generated from EDMX file: C:\Users\lazarusa\Documents\Git Projects\SWIMS\Model_Designer\Model.edmx
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


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'SW_users'
CREATE TABLE [dbo].[SW_users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [fullName] nvarchar(max)  NOT NULL,
    [userName] nvarchar(max)  NOT NULL,
    [email] nvarchar(max)  NOT NULL,
    [password] nvarchar(max)  NOT NULL,
    [SW_rolesId] int  NOT NULL
);
GO

-- Creating table 'SW_roles'
CREATE TABLE [dbo].[SW_roles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'SW_users'
ALTER TABLE [dbo].[SW_users]
ADD CONSTRAINT [PK_SW_users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SW_roles'
ALTER TABLE [dbo].[SW_roles]
ADD CONSTRAINT [PK_SW_roles]
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

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------