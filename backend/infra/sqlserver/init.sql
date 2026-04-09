-- FinVault SQL Server Initialization Script
-- Creates all service databases if they don't already exist

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'finvault_identity')
BEGIN
    CREATE DATABASE finvault_identity;
    PRINT 'Created database: finvault_identity';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'finvault_cards')
BEGIN
    CREATE DATABASE finvault_cards;
    PRINT 'Created database: finvault_cards';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'finvault_billing')
BEGIN
    CREATE DATABASE finvault_billing;
    PRINT 'Created database: finvault_billing';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'finvault_payments')
BEGIN
    CREATE DATABASE finvault_payments;
    PRINT 'Created database: finvault_payments';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'finvault_notifications')
BEGIN
    CREATE DATABASE finvault_notifications;
    PRINT 'Created database: finvault_notifications';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'finvault_users')
BEGIN
    CREATE DATABASE finvault_users;
    PRINT 'Created database: finvault_users';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'finvault_transactions')
BEGIN
    CREATE DATABASE finvault_transactions;
    PRINT 'Created database: finvault_transactions';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'finvault_rewards')
BEGIN
    CREATE DATABASE finvault_rewards;
    PRINT 'Created database: finvault_rewards';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'finvault_support')
BEGIN
    CREATE DATABASE finvault_support;
    PRINT 'Created database: finvault_support';
END
GO
