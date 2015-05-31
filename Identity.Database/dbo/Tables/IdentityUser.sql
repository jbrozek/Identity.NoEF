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

