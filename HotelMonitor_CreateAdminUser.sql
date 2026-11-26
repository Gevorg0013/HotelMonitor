/*
    HotelMonitor - create the initial 'admin' account in VTM.Db
    Table     : dbo.Auth (EF entity AuthUser is mapped to dbo.Auth; there is no dbo.AuthUser table)
    Password  : hashed with ASP.NET Core Identity PasswordHasher<AuthUser> (the hasher registered in Startup.cs)
                V3 format: PBKDF2-HMAC-SHA512, 100,000 iterations, 128-bit random salt, 256-bit subkey
    Effect    : inserts one row only if 'admin' does not exist; never updates or deletes existing users
    Id        : INT IDENTITY, assigned by SQL Server
*/

USE [VTM.Db];
GO

SET XACT_ABORT ON;
SET NOCOUNT ON;
GO

DECLARE @Username     nvarchar(100) = N'admin';
DECLARE @PasswordHash nvarchar(512) = N'AQAAAAIAAYagAAAAEO5ma054a0bHb7XZRRdrYxMhyLCcaF7FB+UTiBZSe+SIuw+haC624BUml6S+gLNvJQ==';

IF DB_NAME() <> N'VTM.Db'
    THROW 50000, N'Wrong database: this script must run against VTM.Db. Nothing was changed.', 1;

IF OBJECT_ID(N'[dbo].[Auth]', N'U') IS NULL
    THROW 50001, N'dbo.Auth does not exist. Run dotnet ef database update first. Nothing was changed.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    -- UPDLOCK + HOLDLOCK keeps the check and the insert atomic against concurrent runs.
    IF EXISTS (SELECT 1 FROM [dbo].[Auth] WITH (UPDLOCK, HOLDLOCK) WHERE [Username] = @Username)
    BEGIN
        PRINT N'User ''' + @Username + N''' already exists. Nothing was changed.';
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[Auth] ([Username], [PasswordHash], [IsActive], [CreatedAt])
        VALUES (@Username, @PasswordHash, 1, SYSUTCDATETIME());

        PRINT N'User ''' + @Username + N''' created.';
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT N'Creating user ''' + @Username + N''' FAILED and was rolled back. Nothing was changed.';
    THROW;
END CATCH;
GO

-- Optional verification (read-only):
-- SELECT [Id], [Username], [IsActive], [CreatedAt] FROM [dbo].[Auth] WHERE [Username] = N'admin';
