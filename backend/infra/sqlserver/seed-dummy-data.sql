-- =====================================================
-- FinVault Dummy Data Seeding Script
-- =====================================================
-- This script populates all databases with test data
-- Run this after migrations are complete
-- =====================================================

-- =====================================================
-- CARD SERVICE DATA
-- =====================================================
USE finvault_cards;
GO

DECLARE @UserId1 UNIQUEIDENTIFIER = '95e971ac-b77c-4e00-9869-ad4544e1bf57';
DECLARE @UserId2 UNIQUEIDENTIFIER = 'A60C532E-5740-496F-97AE-7CFCC79AD083';
DECLARE @Issuer1 UNIQUEIDENTIFIER;
DECLARE @Card1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Card2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Card3 UNIQUEIDENTIFIER = NEWID();

-- Insert Card Issuers
IF NOT EXISTS (SELECT 1 FROM CardIssuers WHERE Name = 'Visa')
BEGIN
    SET @Issuer1 = NEWID();
    INSERT INTO CardIssuers (Id, Name, CreatedAt)
    VALUES 
        (@Issuer1, 'Visa', GETUTCDATE()),
        (NEWID(), 'Mastercard', GETUTCDATE()),
        (NEWID(), 'American Express', GETUTCDATE());
END
ELSE
BEGIN
    SELECT TOP 1 @Issuer1 = Id FROM CardIssuers WHERE Name = 'Visa';
END

-- Insert Credit Cards for User 1 (Ankit)
IF NOT EXISTS (SELECT 1 FROM CreditCards WHERE UserId = @UserId1)
BEGIN
    INSERT INTO CreditCards (
        Id, UserId, MaskedNumber, CardholderName, ExpiryMonth, ExpiryYear, 
        IssuerId, CreditLimit, OutstandingBalance, BillingCycleStartDay,
        IsDefault, IsVerified, IsDeleted, CreatedAt, UpdatedAt
    )
    VALUES 
    (
        @Card1,
        @UserId1,
        '**** **** **** 9012', -- Visa test card
        'ANKIT RANJAN',
        12,
        2028,
        @Issuer1,
        50000.00,
        15000.00,
        1,
        1, -- Default card
        1, -- Verified
        0, -- Not deleted
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        @Card2,
        @UserId1,
        '**** **** **** 9903', -- Mastercard test card
        'ANKIT RANJAN',
        6,
        2029,
        @Issuer1,
        100000.00,
        15000.00,
        1,
        0,
        1,
        0,
        GETUTCDATE(),
        GETUTCDATE()
    );
    PRINT 'Inserted 2 cards for User 1';
END
GO

-- Insert Credit Cards for User 2 (Direct User)
DECLARE @UserId2 UNIQUEIDENTIFIER = 'A60C532E-5740-496F-97AE-7CFCC79AD083';
DECLARE @Card3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Issuer1 UNIQUEIDENTIFIER;

SELECT TOP 1 @Issuer1 = Id FROM CardIssuers WHERE Name = 'Visa';

IF NOT EXISTS (SELECT 1 FROM CreditCards WHERE UserId = @UserId2)
BEGIN
    INSERT INTO CreditCards (
        Id, UserId, MaskedNumber, CardholderName, ExpiryMonth, ExpiryYear, 
        IssuerId, CreditLimit, OutstandingBalance, BillingCycleStartDay,
        IsDefault, IsVerified, IsDeleted, CreatedAt, UpdatedAt
    )
    VALUES 
    (
        @Card3,
        @UserId2,
        '**** ****** *10005', -- Amex test card
        'DIRECT USER',
        9,
        2027,
        @Issuer1,
        75000.00,
        15000.00,
        1,
        1,
        1,
        0,
        GETUTCDATE(),
        GETUTCDATE()
    );
    PRINT 'Inserted 1 card for User 2';
END
GO

-- =====================================================
-- PAYMENT SERVICE DATA
-- =====================================================
USE finvault_payments;
GO

DECLARE @UserId1 UNIQUEIDENTIFIER = '95e971ac-b77c-4e00-9869-ad4544e1bf57';
DECLARE @UserId2 UNIQUEIDENTIFIER = 'A60C532E-5740-496F-97AE-7CFCC79AD083';
DECLARE @Card1 UNIQUEIDENTIFIER;
DECLARE @Card2 UNIQUEIDENTIFIER;

-- Get card IDs from card service
SELECT TOP 1 @Card1 = Id FROM finvault_cards.dbo.CreditCards WHERE UserId = @UserId1 AND IsDefault = 1;
SELECT TOP 1 @Card2 = Id FROM finvault_cards.dbo.CreditCards WHERE UserId = @UserId2 AND IsDefault = 1;

-- Insert Payments for User 1
IF NOT EXISTS (SELECT 1 FROM Payments WHERE UserId = @UserId1)
BEGIN
    INSERT INTO Payments (
        Id, UserId, CardId, Amount, Currency, 
        Status, TransactionDate, ExternalTransactionId, AuthorizationCode
    )
    VALUES 
    (
        NEWID(),
        @UserId1,
        @Card1,
        2500.00,
        'USD',
        'Completed',
        DATEADD(DAY, -5, GETUTCDATE()),
        'TXN-' + CAST(NEWID() AS VARCHAR(36)),
        'AUTH-' + LEFT(CAST(NEWID() AS VARCHAR(36)), 8)
    ),
    (
        NEWID(),
        @UserId1,
        @Card1,
        1200.50,
        'USD',
        'Completed',
        DATEADD(DAY, -3, GETUTCDATE()),
        'TXN-' + CAST(NEWID() AS VARCHAR(36)),
        'AUTH-' + LEFT(CAST(NEWID() AS VARCHAR(36)), 8)
    ),
    (
        NEWID(),
        @UserId1,
        @Card1,
        500.00,
        'USD',
        'Pending',
        DATEADD(HOUR, -2, GETUTCDATE()),
        'TXN-' + CAST(NEWID() AS VARCHAR(36)),
        NULL
    );
    PRINT 'Inserted 3 payments for User 1';
END
GO

-- Insert Payments for User 2
DECLARE @UserId2 UNIQUEIDENTIFIER = 'A60C532E-5740-496F-97AE-7CFCC79AD083';
DECLARE @Card2 UNIQUEIDENTIFIER;

SELECT TOP 1 @Card2 = Id FROM finvault_cards.dbo.CreditCards WHERE UserId = @UserId2 AND IsDefault = 1;

IF NOT EXISTS (SELECT 1 FROM Payments WHERE UserId = @UserId2)
BEGIN
    INSERT INTO Payments (
        Id, UserId, CardId, Amount, Currency, 
        Status, TransactionDate, ExternalTransactionId, AuthorizationCode
    )
    VALUES 
    (
        NEWID(),
        @UserId2,
        @Card2,
        3500.00,
        'USD',
        'Completed',
        DATEADD(DAY, -10, GETUTCDATE()),
        'TXN-' + CAST(NEWID() AS VARCHAR(36)),
        'AUTH-' + LEFT(CAST(NEWID() AS VARCHAR(36)), 8)
    );
    PRINT 'Inserted 1 payment for User 2';
END
GO

-- =====================================================
-- NOTIFICATION SERVICE DATA
-- =====================================================
USE finvault_notifications;
GO

DECLARE @UserId1 UNIQUEIDENTIFIER = '95e971ac-b77c-4e00-9869-ad4544e1bf57';
DECLARE @UserId2 UNIQUEIDENTIFIER = 'A60C532E-5740-496F-97AE-7CFCC79AD083';

-- Insert Notification Templates
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates)
BEGIN
    INSERT INTO NotificationTemplates (Id, Name, Type, SubjectTemplate, BodyTemplate, CreatedAt)
    VALUES 
        (NEWID(), 'PaymentSuccess', 'Email', 'Payment Successful', 'Your payment of {amount} has been processed successfully.', GETUTCDATE()),
        (NEWID(), 'CardActivated', 'Email', 'Card Activated', 'Your credit card ending in {last4} has been activated.', GETUTCDATE()),
        (NEWID(), 'LowBalance', 'Email', 'Low Balance Alert', 'Your available credit is below {threshold}.', GETUTCDATE());
    PRINT 'Inserted notification templates';
END
GO

-- Insert Notifications for User 1
DECLARE @UserId1 UNIQUEIDENTIFIER = '95e971ac-b77c-4e00-9869-ad4544e1bf57';

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE UserId = @UserId1)
BEGIN
    INSERT INTO Notifications (
        Id, UserId, Type, Message, IsRead, SentDate, ActionUrl
    )
    VALUES 
    (
        NEWID(),
        @UserId1,
        'Payment',
        'Your payment of $2,500.00 to Amazon has been processed successfully.',
        1, -- Read
        DATEADD(DAY, -5, GETUTCDATE()),
        NULL
    ),
    (
        NEWID(),
        @UserId1,
        'Payment',
        'Your payment of $1,200.50 to Walmart has been processed successfully.',
        1, -- Read
        DATEADD(DAY, -3, GETUTCDATE()),
        NULL
    ),
    (
        NEWID(),
        @UserId1,
        'Card',
        'Your Mastercard ending in 9903 has been activated and is ready to use.',
        0, -- Unread
        DATEADD(HOUR, -6, GETUTCDATE()),
        '/cards'
    ),
    (
        NEWID(),
        @UserId1,
        'Alert',
        'Your Visa card available credit is below $40,000. Consider making a payment.',
        0, -- Unread
        DATEADD(HOUR, -1, GETUTCDATE()),
        '/payments'
    );
    PRINT 'Inserted 4 notifications for User 1';
END
GO

-- Insert Notifications for User 2
DECLARE @UserId2 UNIQUEIDENTIFIER = 'A60C532E-5740-496F-97AE-7CFCC79AD083';

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE UserId = @UserId2)
BEGIN
    INSERT INTO Notifications (
        Id, UserId, Type, Message, IsRead, SentDate, ActionUrl
    )
    VALUES 
    (
        NEWID(),
        @UserId2,
        'Payment',
        'Your payment of $3,500.00 for flight booking has been processed successfully.',
        1, -- Read
        DATEADD(DAY, -10, GETUTCDATE()),
        NULL
    );
    PRINT 'Inserted 1 notification for User 2';
END
GO

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================
PRINT ''
PRINT '=== VERIFICATION ==='
PRINT ''

USE finvault_cards;
SELECT 'Cards' AS Table_Name, COUNT(*) AS Record_Count FROM CreditCards;
SELECT 'Card Issuers' AS Table_Name, COUNT(*) AS Record_Count FROM CardIssuers;

USE finvault_payments;
SELECT 'Payments' AS Table_Name, COUNT(*) AS Record_Count FROM Payments;

USE finvault_notifications;
SELECT 'Notifications' AS Table_Name, COUNT(*) AS Record_Count FROM Notifications;
SELECT 'Notification Templates' AS Table_Name, COUNT(*) AS Record_Count FROM NotificationTemplates;

PRINT ''
PRINT '=== DUMMY DATA SEEDING COMPLETED ==='
GO

