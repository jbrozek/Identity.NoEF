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

