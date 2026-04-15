-- Add encrypted card fields to CreditCards table
USE finvault_cards;
GO

-- Check if columns don't exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CreditCards]') AND name = 'EncryptedCardNumber')
BEGIN
    ALTER TABLE CreditCards
    ADD EncryptedCardNumber NVARCHAR(500) NULL;
    PRINT 'Added EncryptedCardNumber column';
END
ELSE
BEGIN
    PRINT 'EncryptedCardNumber column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CreditCards]') AND name = 'EncryptedCVV')
BEGIN
    ALTER TABLE CreditCards
    ADD EncryptedCVV NVARCHAR(100) NULL;
    PRINT 'Added EncryptedCVV column';
END
ELSE
BEGIN
    PRINT 'EncryptedCVV column already exists';
END

GO
PRINT 'Migration completed successfully';
