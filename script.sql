-- Drop and recreate the managed identity user with correct permissions
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'mid-AppModAssist')
BEGIN
    DROP USER [mid-AppModAssist];
END
GO

CREATE USER [mid-AppModAssist] FROM EXTERNAL PROVIDER;
GO

ALTER ROLE db_datareader ADD MEMBER [mid-AppModAssist];
GO

ALTER ROLE db_datawriter ADD MEMBER [mid-AppModAssist];
GO
