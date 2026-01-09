using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;

var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SMSPrototype2;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true";

var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new DataContext(optionsBuilder.Options);

Console.WriteLine("Starting database migration...");

try
{
    // Check if migration is needed
    var checkSql = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'PasswordHash'";
    var result = await context.Database.ExecuteSqlRawAsync($"IF NOT EXISTS ({checkSql}) SELECT 1 ELSE SELECT 0");
    
    // Add new columns
    Console.WriteLine("Adding new columns...");
    await context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'PasswordHash') ALTER TABLE ChatRooms ADD PasswordHash NVARCHAR(MAX) NULL");
    await context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'CreatedByUsername') ALTER TABLE ChatRooms ADD CreatedByUsername NVARCHAR(MAX) NULL");
    await context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'CreatedAt') ALTER TABLE ChatRooms ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()");
    await context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'LastActivityAt') ALTER TABLE ChatRooms ADD LastActivityAt DATETIME2 NULL");
    await context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'IsActive') ALTER TABLE ChatRooms ADD IsActive BIT NOT NULL DEFAULT 1");
    await context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'PrivacyLevel') ALTER TABLE ChatRooms ADD PrivacyLevel INT NOT NULL DEFAULT 1");
    await context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'MaxParticipants') ALTER TABLE ChatRooms ADD MaxParticipants INT NOT NULL DEFAULT 50");
    await context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'AllowRecording') ALTER TABLE ChatRooms ADD AllowRecording BIT NOT NULL DEFAULT 1");
    await context.Database.ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'IsEncrypted') ALTER TABLE ChatRooms ADD IsEncrypted BIT NOT NULL DEFAULT 1");
    
    // Migrate existing data
    Console.WriteLine("Migrating existing data...");
    await context.Database.ExecuteSqlRawAsync("UPDATE ChatRooms SET PasswordHash = Password, LastActivityAt = GETUTCDATE() WHERE PasswordHash IS NULL AND Password IS NOT NULL");
    
    // Update constraints
    Console.WriteLine("Updating column constraints...");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE ChatRooms ALTER COLUMN [Name] NVARCHAR(100) NOT NULL");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE ChatRooms ALTER COLUMN [Description] NVARCHAR(500) NULL");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE ChatRooms ALTER COLUMN PasswordHash NVARCHAR(MAX) NOT NULL");
    
    // Drop old column
    Console.WriteLine("Dropping old Password column...");
    await context.Database.ExecuteSqlRawAsync("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatRooms' AND COLUMN_NAME = 'Password') ALTER TABLE ChatRooms DROP COLUMN Password");
    
    Console.WriteLine("✓ Database migration completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Migration failed: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    return 1;
}

return 0;
