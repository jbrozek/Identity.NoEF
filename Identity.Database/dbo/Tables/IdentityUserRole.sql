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

