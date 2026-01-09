-- Add ChatMessages table for SignalR chat persistence
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [RoomId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [Content] NVARCHAR(1000) NOT NULL,
        [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [IsEdited] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_ChatMessages_ChatRooms] FOREIGN KEY ([RoomId]) REFERENCES [ChatRooms]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatMessages_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_ChatMessages_RoomId] ON [dbo].[ChatMessages]([RoomId]);
    CREATE INDEX [IX_ChatMessages_UserId] ON [dbo].[ChatMessages]([UserId]);
    CREATE INDEX [IX_ChatMessages_Timestamp] ON [dbo].[ChatMessages]([Timestamp] DESC);
    
    PRINT 'ChatMessages table created successfully';
END
ELSE
BEGIN
    PRINT 'ChatMessages table already exists';
END
GO

-- Fix ChatRoomUsers.RoomId from string to Guid if needed
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ChatRoomUsers' AND COLUMN_NAME = 'RoomId' AND DATA_TYPE = 'nvarchar')
BEGIN
    -- Drop foreign key constraint if exists
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatRoomUsers_ChatRooms_RoomId')
    BEGIN
        ALTER TABLE [dbo].[ChatRoomUsers] DROP CONSTRAINT [FK_ChatRoomUsers_ChatRooms_RoomId];
    END
    
    -- Drop existing index if exists
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ChatRoomUsers_RoomId')
    BEGIN
        DROP INDEX [IX_ChatRoomUsers_RoomId] ON [dbo].[ChatRoomUsers];
    END
    
    -- Alter column type
    ALTER TABLE [dbo].[ChatRoomUsers] ALTER COLUMN [RoomId] UNIQUEIDENTIFIER NOT NULL;
    
    -- Recreate foreign key
    ALTER TABLE [dbo].[ChatRoomUsers] 
        ADD CONSTRAINT [FK_ChatRoomUsers_ChatRooms_RoomId] 
        FOREIGN KEY ([RoomId]) REFERENCES [ChatRooms]([Id]) ON DELETE CASCADE;
    
    -- Recreate index
    CREATE INDEX [IX_ChatRoomUsers_RoomId] ON [dbo].[ChatRoomUsers]([RoomId]);
    
    PRINT 'ChatRoomUsers.RoomId fixed to UNIQUEIDENTIFIER';
END
ELSE
BEGIN
    PRINT 'ChatRoomUsers.RoomId is already UNIQUEIDENTIFIER or table does not exist';
END
GO

PRINT 'Database migration completed successfully!';
