-- Supplemental Seed script for Admin Card Dashboard
USE finvault_cards;
GO

-- Add a few more cards if they don't exist, specifically for testing admin views
-- These cards are from different users to populate the 'All Cards' view

DECLARE @UserId3 UNIQUEIDENTIFIER = NEWID(); -- Mock user 3
DECLARE @Issuer1 UNIQUEIDENTIFIER;

SELECT TOP 1 @Issuer1 = Id FROM CardIssuers WHERE Name = 'Visa';

IF NOT EXISTS (SELECT 1 FROM CreditCards WHERE MaskedNumber = '**** **** **** 1111')
BEGIN
    INSERT INTO CreditCards (
        Id, UserId, MaskedNumber, CardholderName, ExpiryMonth, ExpiryYear, 
        IssuerId, CreditLimit, OutstandingBalance, BillingCycleStartDay,
        IsDefault, IsVerified, IsDeleted, CreatedAt, UpdatedAt
    )
    VALUES 
    (
        NEWID(),
        @UserId3,
        '**** **** **** 1111',
        'SARAH CONNOR',
        10,
        2030,
        @Issuer1,
        250000.00,
        450.00,
        15,
        1,
        0, -- PENDING verification
        0,
        GETUTCDATE(),
        GETUTCDATE()
    );
END

IF NOT EXISTS (SELECT 1 FROM CreditCards WHERE MaskedNumber = '**** **** **** 8888')
BEGIN
    INSERT INTO CreditCards (
        Id, UserId, MaskedNumber, CardholderName, ExpiryMonth, ExpiryYear, 
        IssuerId, CreditLimit, OutstandingBalance, BillingCycleStartDay,
        IsDefault, IsVerified, IsDeleted, CreatedAt, UpdatedAt
    )
    VALUES 
    (
        NEWID(),
        NEWID(), -- Random user
        '**** **** **** 8888',
        'BRUCE WAYNE',
        1,
        2032,
        @Issuer1,
        1000000.00,
        0.00,
        1,
        1,
        0, -- PENDING
        0,
        GETUTCDATE(),
        GETUTCDATE()
    );
END

PRINT 'Admin test cards seeded.';
GO
