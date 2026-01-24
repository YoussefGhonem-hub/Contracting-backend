BEGIN TRANSACTION;
GO

CREATE TABLE [PasswordResetCodes] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Code] nvarchar(max) NOT NULL,
    [ExpiresAt] datetimeoffset NOT NULL,
    [IsVerified] bit NOT NULL,
    [IsUsed] bit NOT NULL,
    [VerifiedAt] datetimeoffset NULL,
    [UsedAt] datetimeoffset NULL,
    [CreatedDate] datetimeoffset NOT NULL,
    [ModifiedDate] datetimeoffset NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [ModifiedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedDate] datetimeoffset NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_PasswordResetCodes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PasswordResetCodes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [security].[Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_PasswordResetCodes_IsDeleted] ON [PasswordResetCodes] ([IsDeleted]);
GO

CREATE INDEX [IX_PasswordResetCodes_UserId] ON [PasswordResetCodes] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260117080439_AddPasswordResetCodeTable', N'8.0.8');
GO

COMMIT;
GO

