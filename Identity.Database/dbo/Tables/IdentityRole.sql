CREATE TABLE [dbo].[IdentityRole] (
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    [Name]   NVARCHAR (256)   NOT NULL,
    CONSTRAINT [PK_dbo.IdentityRole] PRIMARY KEY CLUSTERED ([RoleId] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[IdentityRole]([Name] ASC);

