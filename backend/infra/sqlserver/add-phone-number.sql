-- Add PhoneNumber field to Users table
USE finvault_identity;
GO

-- Check if column doesn't exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE Users
    ADD PhoneNumber NVARCHAR(20) NULL;
    PRINT 'Added PhoneNumber column';
END
ELSE
BEGIN
    PRINT 'PhoneNumber column already exists';
END

GO
PRINT 'Migration completed successfully';
