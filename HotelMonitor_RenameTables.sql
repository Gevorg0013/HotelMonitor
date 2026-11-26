BEGIN TRANSACTION;
ALTER TABLE [ImageTable] DROP CONSTRAINT [PK_ImageTable];

ALTER TABLE [Auth] DROP CONSTRAINT [PK_Auth];

EXEC sp_rename N'[ImageTable]', N'Images', 'OBJECT';

EXEC sp_rename N'[Auth]', N'IdentityMonitorManager', 'OBJECT';

EXEC sp_rename N'[Images].[IX_ImageTable_SequenceNumber]', N'IX_Images_SequenceNumber', 'INDEX';

EXEC sp_rename N'[IdentityMonitorManager].[IX_Auth_Username]', N'IX_IdentityMonitorManager_Username', 'INDEX';

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Images]') AND [c].[name] = N'SequenceNumber');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Images] DROP CONSTRAINT ' + @var + ';');

DROP SEQUENCE [ImageSequenceNumbers];

ALTER TABLE [Images] ADD CONSTRAINT [PK_Images] PRIMARY KEY ([Id]);

ALTER TABLE [IdentityMonitorManager] ADD CONSTRAINT [PK_IdentityMonitorManager] PRIMARY KEY ([Id]);

INSERT INTO [__HotelMonitorMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261126220144_RenameTablesAndClientSequenceNumber', N'10.0.13');

COMMIT;
GO

