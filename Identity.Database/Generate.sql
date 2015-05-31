CREATE TABLE [dbo].[IdentityRole] (
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    [Name]   NVARCHAR (256)   NOT NULL,
    CONSTRAINT [PK_dbo.IdentityRole] PRIMARY KEY CLUSTERED ([RoleId] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[IdentityRole]([Name] ASC);

GO
CREATE TABLE [dbo].[IdentityUser] (
    [UserId]               UNIQUEIDENTIFIER NOT NULL,
    [Email]                NVARCHAR (256)   NULL,
    [EmailConfirmed]       BIT              NOT NULL,
    [PasswordHash]         NVARCHAR (MAX)   NULL,
    [SecurityStamp]        NVARCHAR (MAX)   NULL,
    [PhoneNumber]          NVARCHAR (MAX)   NULL,
    [PhoneNumberConfirmed] BIT              NOT NULL,
    [TwoFactorEnabled]     BIT              NOT NULL,
    [LockoutEndDateUtc]    DATETIME         NULL,
    [LockoutEnabled]       BIT              NOT NULL,
    [AccessFailedCount]    INT              NOT NULL,
    [UserName]             NVARCHAR (256)   NOT NULL,
    [CreateBy]             UNIQUEIDENTIFIER NOT NULL,
    [CreateDate]           SMALLDATETIME    CONSTRAINT [DF_IdentityUser_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [ModifyBy]             UNIQUEIDENTIFIER NOT NULL,
    [ModifyDate]           SMALLDATETIME    CONSTRAINT [DF_IdentityUser_ModifyDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_dbo.IdentityUser] PRIMARY KEY CLUSTERED ([UserId] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[IdentityUser]([UserName] ASC);

GO
CREATE TABLE [dbo].[IdentityProfile] (
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [FirstName]  VARCHAR (128)    NULL,
    [MiddleName] VARCHAR (128)    NULL,
    [LastName]   VARCHAR (128)    NULL,
    [CreateBy]   UNIQUEIDENTIFIER NOT NULL,
    [CreateDate] SMALLDATETIME    CONSTRAINT [DF_IdentityUserProfile_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [ModifyBy]   UNIQUEIDENTIFIER NOT NULL,
    [ModifyDate] SMALLDATETIME    CONSTRAINT [DF_IdentityUserProfile_ModifyDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_IdentityProfile] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_IdentityProfile_IdentityUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[IdentityUser] ([UserId]) ON DELETE CASCADE
);

GO
CREATE TABLE [dbo].[IdentityUserRole] (
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_dbo.IdentityUserRole] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo.IdentityUserRole_dbo.IdentityRole_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[IdentityRole] ([RoleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.IdentityUserRole_dbo.IdentityUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[IdentityUser] ([UserId]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[IdentityUserRole]([UserId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_RoleId]
    ON [dbo].[IdentityUserRole]([RoleId] ASC);

GO
CREATE TABLE [dbo].[IdentityLogin] (
    [LoginProvider] NVARCHAR (128)   NOT NULL,
    [ProviderKey]   NVARCHAR (128)   NOT NULL,
    [UserId]        UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_IdentityLogin] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC),
    CONSTRAINT [FK_dbo.IdentityLogin_dbo.IdentityUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[IdentityUser] ([UserId]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[IdentityLogin]([UserId] ASC);

GO
CREATE TABLE [dbo].[IdentityClaim] (
    [ClaimId]    UNIQUEIDENTIFIER NOT NULL,
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [ClaimType]  NVARCHAR (MAX)   NULL,
    [ClaimValue] NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_dbo.IdentityClaim] PRIMARY KEY CLUSTERED ([ClaimId] ASC),
    CONSTRAINT [FK_dbo.IdentityClaim_dbo.IdentityUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[IdentityUser] ([UserId]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[IdentityClaim]([UserId] ASC);