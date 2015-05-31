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

