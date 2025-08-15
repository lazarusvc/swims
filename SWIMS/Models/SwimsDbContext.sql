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
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE TABLE [SW_roles] (
        [Id] int NOT NULL IDENTITY,
        [name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_SW_roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE TABLE [SW_users] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_SW_users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE TABLE [SW_role_claims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_SW_role_claims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SW_role_claims_SW_roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [SW_roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE TABLE [SW_user_claims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_SW_user_claims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SW_user_claims_SW_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [SW_users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE TABLE [SW_user_logins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_SW_user_logins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_SW_user_logins_SW_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [SW_users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE TABLE [SW_user_roles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_SW_user_roles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_SW_user_roles_SW_roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [SW_roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SW_user_roles_SW_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [SW_users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE TABLE [SW_user_tokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_SW_user_tokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_SW_user_tokens_SW_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [SW_users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE INDEX [IX_SW_role_claims_RoleId] ON [SW_role_claims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [SW_roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE INDEX [IX_SW_user_claims_UserId] ON [SW_user_claims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE INDEX [IX_SW_user_logins_UserId] ON [SW_user_logins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE INDEX [IX_SW_user_roles_RoleId] ON [SW_user_roles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [SW_users] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [SW_users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814151309_DB Context More'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250814151309_DB Context More', N'9.0.8');
END;

COMMIT;
GO