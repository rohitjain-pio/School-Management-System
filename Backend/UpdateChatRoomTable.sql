-- Update ChatRoom table for security enhancements
-- Run this script to update existing database

-- First, check if we need to rename the Password column
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'Password')
BEGIN
    PRINT 'Updating ChatRoom table schema...'
    
    -- Add new columns if they don't exist
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'PasswordHash')
    BEGIN
        ALTER TABLE ChatRooms ADD PasswordHash NVARCHAR(MAX) NULL;
        PRINT 'Added PasswordHash column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'CreatedByUsername')
    BEGIN
        ALTER TABLE ChatRooms ADD CreatedByUsername NVARCHAR(MAX) NULL;
        PRINT 'Added CreatedByUsername column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'CreatedAt')
    BEGIN
        ALTER TABLE ChatRooms ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added CreatedAt column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'LastActivityAt')
    BEGIN
        ALTER TABLE ChatRooms ADD LastActivityAt DATETIME2 NULL;
        PRINT 'Added LastActivityAt column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'IsActive')
    BEGIN
        ALTER TABLE ChatRooms ADD IsActive BIT NOT NULL DEFAULT 1;
        PRINT 'Added IsActive column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'PrivacyLevel')
    BEGIN
        ALTER TABLE ChatRooms ADD PrivacyLevel INT NOT NULL DEFAULT 1;
        PRINT 'Added PrivacyLevel column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'MaxParticipants')
    BEGIN
        ALTER TABLE ChatRooms ADD MaxParticipants INT NOT NULL DEFAULT 50;
        PRINT 'Added MaxParticipants column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'AllowRecording')
    BEGIN
        ALTER TABLE ChatRooms ADD AllowRecording BIT NOT NULL DEFAULT 1;
        PRINT 'Added AllowRecording column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'IsEncrypted')
    BEGIN
        ALTER TABLE ChatRooms ADD IsEncrypted BIT NOT NULL DEFAULT 1;
        PRINT 'Added IsEncrypted column'
    END
    
    -- Migrate existing plain passwords to hashed passwords (BCrypt hash of the plain password)
    -- Note: Existing rooms will need password reset as we can't reverse-hash plain passwords
    -- For now, we'll just copy to PasswordHash (you should ask users to reset passwords)
    UPDATE ChatRooms 
    SET PasswordHash = Password, 
        CreatedAt = GETUTCDATE(),
        LastActivityAt = GETUTCDATE(),
        IsActive = 1,
        PrivacyLevel = 1,
        MaxParticipants = 50,
        AllowRecording = 1,
        IsEncrypted = 1
    WHERE PasswordHash IS NULL;
    
    PRINT 'Migrated existing data'
    
    -- Now modify Name column constraints if needed
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'Name' 
               AND (CHARACTER_MAXIMUM_LENGTH IS NULL OR CHARACTER_MAXIMUM_LENGTH <> 100))
    BEGIN
        ALTER TABLE ChatRooms ALTER COLUMN [Name] NVARCHAR(100) NOT NULL;
        PRINT 'Updated Name column constraint'
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'Description' 
               AND (CHARACTER_MAXIMUM_LENGTH IS NULL OR CHARACTER_MAXIMUM_LENGTH <> 500))
    BEGIN
        ALTER TABLE ChatRooms ALTER COLUMN [Description] NVARCHAR(500) NULL;
        PRINT 'Updated Description column constraint'
    END
    
    -- Make PasswordHash NOT NULL after data migration
    ALTER TABLE ChatRooms ALTER COLUMN PasswordHash NVARCHAR(MAX) NOT NULL;
    PRINT 'Made PasswordHash required'
    
    -- Drop old Password column
    ALTER TABLE ChatRooms DROP COLUMN Password;
    PRINT 'Dropped old Password column'
    
    PRINT 'ChatRoom table update complete!'
    PRINT ''
    PRINT 'IMPORTANT: Existing room passwords are NOT hashed.'
    PRINT 'Users will need to reset their room passwords on next login attempt.'
END
ELSE
BEGIN
    PRINT 'ChatRoom table already migrated or Password column not found'
END
