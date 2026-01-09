-- Manual SQL Script to Seed Data into SMSPrototype2 Database
-- Run this script in SQL Server Management Studio or Azure Data Studio

USE SMSPrototype2;
GO

-- Insert Schools (only if not exists)
IF NOT EXISTS (SELECT 1 FROM Schools WHERE RegistrationNumber = 'SCH001')
BEGIN
    INSERT INTO Schools (Id, RegistrationNumber, Name, Email, Phone, Address, City, State, PinCode, Subscription, SubscriptionDate, IsSoftDeleted)
    VALUES 
        (NEWID(), 'SCH001', 'Greenwood High School', 'contact@greenwoodhigh.com', '9876543210', '123 Education Lane', 'Springfield', 'California', 900001, 1, CAST(GETDATE() AS DATE), 0),
        (NEWID(), 'SCH002', 'Sunrise Academy', 'info@sunriseacademy.com', '9123456789', '456 Knowledge Street', 'Riverside', 'Texas', 750001, 1, CAST(DATEADD(MONTH, -2, GETDATE()) AS DATE), 0),
        (NEWID(), 'SCH003', 'Maple Leaf International School', 'admin@mapleleaf.edu', '9234567890', '789 Learning Boulevard', 'Portland', 'Oregon', 970001, 1, CAST(DATEADD(MONTH, -1, GETDATE()) AS DATE), 0);
    
    PRINT '✓ Schools seeded successfully';
END
ELSE
BEGIN
    PRINT '✓ Schools already exist';
END
GO

-- Get the first school ID for user associations
DECLARE @SchoolId UNIQUEIDENTIFIER;
SELECT TOP 1 @SchoolId = Id FROM Schools WHERE RegistrationNumber = 'SCH001';

-- Insert Roles (if not exists)
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SuperAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES 
        (NEWID(), 'SuperAdmin', 'SUPERADMIN', NEWID()),
        (NEWID(), 'Admin', 'ADMIN', NEWID()),
        (NEWID(), 'Teacher', 'TEACHER', NEWID()),
        (NEWID(), 'Student', 'STUDENT', NEWID()),
        (NEWID(), 'Parent', 'PARENT', NEWID());
    
    PRINT '✓ Roles seeded successfully';
END
ELSE
BEGIN
    PRINT '✓ Roles already exist';
END
GO

-- NOTE: To create users with proper password hashing, you need to run the application
-- ASP.NET Identity uses PBKDF2 with random salt for password hashing which cannot be easily replicated in SQL

PRINT '';
PRINT '===========================================';
PRINT 'Database seeding partially complete!';
PRINT '===========================================';
PRINT '';
PRINT 'Schools have been seeded.';
PRINT 'Roles have been seeded.';
PRINT '';
PRINT 'To seed users with proper password hashing,';
PRINT 'you MUST run the application at least once.';
PRINT '';
PRINT 'Try running: dotnet run --project SMSPrototype1';
PRINT '';
PRINT 'Or press F5 in VS Code to debug/run.';
PRINT '===========================================';
GO
