# Workflow: Add New Database Table
**Estimated Time:** 45-60 minutes  
**Complexity:** Medium  
**Prerequisites:** Database context understanding, EF Core basics

---

## 📋 Overview

This workflow guides you through creating a new database table with proper multi-tenant isolation, indexes, and migrations.

**What You'll Create:**
1. Entity model class
2. Entity configuration (fluent API)
3. EF Core migration
4. Seed data (optional)
5. Repository interface & implementation

---

## ⏱️ Time-Boxed Steps

### Step 1: Define Entity Model (10 minutes)

**Location:** `SMSDataModel/Model/{EntityName}.cs`

**Template:**
```csharp
namespace SMSDataModel.Model
{
    public class {EntityName}
    {
        // Primary Key
        public Guid Id { get; set; }

        // Multi-tenant Discriminator (MANDATORY!)
        public Guid SchoolId { get; set; }

        // Business Properties
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Status Flag (soft delete)
        public bool IsActive { get; set; } = true;

        // Audit Fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation Properties
        public School School { get; set; } = null!;
        
        // Add foreign key properties
        // public Guid RelatedEntityId { get; set; }
        // public RelatedEntity RelatedEntity { get; set; } = null!;
    }
}
```

**Checklist:**
- [ ] Guid Id (primary key)
- [ ] Guid SchoolId (tenant isolation)
- [ ] IsActive flag (soft delete)
- [ ] CreatedAt, UpdatedAt (audit trail)
- [ ] Navigation properties properly typed
- [ ] All strings have default empty initializers

---

### Step 2: Configure Entity (10 minutes)

**Location:** `SMSDataContext/Data/Configurations/{EntityName}Configuration.cs`

**Template:**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMSDataModel.Model;

namespace SMSDataContext.Data.Configurations
{
    public class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
    {
        public void Configure(EntityTypeBuilder<{EntityName}> builder)
        {
            // Table name
            builder.ToTable("{EntityNames}"); // Plural

            // Primary key
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            // Required fields
            builder.Property(e => e.SchoolId)
                .IsRequired();

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .HasMaxLength(1000);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign Keys
            builder.HasOne(e => e.School)
                .WithMany()
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes (CRITICAL for multi-tenant performance!)
            builder.HasIndex(e => e.SchoolId)
                .HasDatabaseName("IX_{EntityName}_SchoolId");

            builder.HasIndex(e => new { e.SchoolId, e.IsActive })
                .HasDatabaseName("IX_{EntityName}_SchoolId_IsActive");

            // Unique Constraints (within school)
            // builder.HasIndex(e => new { e.Name, e.SchoolId })
            //     .IsUnique()
            //     .HasDatabaseName("UQ_{EntityName}_Name_SchoolId");
        }
    }
}
```

**Checklist:**
- [ ] Table name (plural)
- [ ] Primary key with NEWID()
- [ ] SchoolId required
- [ ] String max lengths defined
- [ ] Foreign keys configured
- [ ] DeleteBehavior appropriate (Cascade/NoAction/SetNull)
- [ ] Index on SchoolId
- [ ] Composite index on SchoolId + IsActive
- [ ] Unique constraints consider SchoolId

---

### Step 3: Register in DbContext (5 minutes)

**Location:** `SMSDataContext/Data/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : DbContext
{
    // Add DbSet
    public DbSet<{EntityName}> {EntityNames} { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configuration
        modelBuilder.ApplyConfiguration(new {EntityName}Configuration());
        
        // ... other configurations
    }
}
```

**Checklist:**
- [ ] DbSet property added
- [ ] Configuration applied in OnModelCreating

---

### Step 4: Create Migration (10 minutes)

**Run in Terminal:**
```powershell
cd Backend/SMSDataContext

# Create migration
dotnet ef migrations add Add{EntityName}Table --startup-project ../SMSPrototype1

# Review generated migration file
code Migrations/*_Add{EntityName}Table.cs
```

**Review Migration:**
```csharp
public partial class Add{EntityName}Table : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "{EntityNames}",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                SchoolId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Description = table.Column<string>(maxLength: 1000, nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<Guid>(nullable: true),
                UpdatedBy = table.Column<Guid>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_{EntityNames}", x => x.Id);
                table.ForeignKey(
                    name: "FK_{EntityNames}_Schools_SchoolId",
                    column: x => x.SchoolId,
                    principalTable: "Schools",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Indexes
        migrationBuilder.CreateIndex(
            name: "IX_{EntityName}_SchoolId",
            table: "{EntityNames}",
            column: "SchoolId");

        migrationBuilder.CreateIndex(
            name: "IX_{EntityName}_SchoolId_IsActive",
            table: "{EntityNames}",
            columns: new[] { "SchoolId", "IsActive" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "{EntityNames}");
    }
}
```

**Checklist:**
- [ ] All columns present with correct types
- [ ] Default values set (NEWID(), GETUTCDATE(), true)
- [ ] Foreign keys defined
- [ ] Indexes created
- [ ] Down() method drops table

**Apply Migration:**
```powershell
# Apply to database
dotnet ef database update --startup-project ../SMSPrototype1
```

---

### Step 5: Create Repository Interface (5 minutes)

**Location:** `SMSRepository/RepositoryInterfaces/I{EntityName}Repository.cs`

```csharp
using SMSDataModel.Model;

namespace SMSRepository.RepositoryInterfaces
{
    public interface I{EntityName}Repository
    {
        Task<IEnumerable<{EntityName}>> GetAllAsync(Guid schoolId);
        Task<{EntityName}?> GetByIdAsync(Guid id, Guid schoolId);
        Task<{EntityName}?> GetByNameAsync(string name, Guid schoolId);
        Task<bool> ExistsAsync(Guid id, Guid schoolId);
        Task AddAsync({EntityName} entity);
        Task UpdateAsync({EntityName} entity);
        Task DeleteAsync({EntityName} entity);
        Task<bool> SaveChangesAsync();
    }
}
```

**Checklist:**
- [ ] All methods have `Guid schoolId` parameter
- [ ] GetByIdAsync returns nullable
- [ ] Async suffix on all methods

---

### Step 6: Implement Repository (10 minutes)

**Location:** `SMSRepository/Repository/{EntityName}Repository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSDataModel.Model;
using SMSRepository.RepositoryInterfaces;

namespace SMSRepository.Repository
{
    public class {EntityName}Repository : I{EntityName}Repository
    {
        private readonly ApplicationDbContext _context;

        public {EntityName}Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<{EntityName}>> GetAllAsync(Guid schoolId)
        {
            return await _context.{EntityNames}
                .Where(e => e.SchoolId == schoolId && e.IsActive == true)
                .OrderBy(e => e.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<{EntityName}?> GetByIdAsync(Guid id, Guid schoolId)
        {
            return await _context.{EntityNames}
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && e.SchoolId == schoolId);
        }

        public async Task<{EntityName}?> GetByNameAsync(string name, Guid schoolId)
        {
            return await _context.{EntityNames}
                .AsNoTracking()
                .FirstOrDefaultAsync(e => 
                    e.Name.ToLower() == name.ToLower() && 
                    e.SchoolId == schoolId);
        }

        public async Task<bool> ExistsAsync(Guid id, Guid schoolId)
        {
            return await _context.{EntityNames}
                .AnyAsync(e => e.Id == id && e.SchoolId == schoolId);
        }

        public async Task AddAsync({EntityName} entity)
        {
            await _context.{EntityNames}.AddAsync(entity);
        }

        public async Task UpdateAsync({EntityName} entity)
        {
            _context.{EntityNames}.Update(entity);
        }

        public async Task DeleteAsync({EntityName} entity)
        {
            // Soft delete
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _context.{EntityNames}.Update(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
```

**Checklist:**
- [ ] All queries filter by SchoolId
- [ ] GetAll uses AsNoTracking
- [ ] GetById uses AsNoTracking
- [ ] Delete is soft delete (IsActive = false)
- [ ] Consistent ordering (OrderBy)

---

### Step 7: Register Repository in DI (5 minutes)

**Location:** `Backend/SMSPrototype1/Program.cs`

```csharp
// Add after other repositories
builder.Services.AddScoped<I{EntityName}Repository, {EntityName}Repository>();
```

**Checklist:**
- [ ] Repository registered with correct lifetime (Scoped)

---

## 🧪 Testing (Optional but Recommended)

### Test Data Seeding

**Location:** Create `Backend/Seed{EntityName}Data.sql`

```sql
-- Seed test data
DECLARE @TestSchoolId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

-- Check if school exists
IF EXISTS (SELECT 1 FROM Schools WHERE Id = @TestSchoolId)
BEGIN
    -- Insert sample records
    INSERT INTO {EntityNames} (Id, SchoolId, Name, Description, IsActive, CreatedAt)
    VALUES
        (NEWID(), @TestSchoolId, 'Sample {EntityName} 1', 'Description 1', 1, GETUTCDATE()),
        (NEWID(), @TestSchoolId, 'Sample {EntityName} 2', 'Description 2', 1, GETUTCDATE()),
        (NEWID(), @TestSchoolId, 'Sample {EntityName} 3', 'Description 3', 1, GETUTCDATE());
    
    PRINT 'Seeded {EntityName} data successfully';
END
ELSE
BEGIN
    PRINT 'Test school not found. Run school seed script first.';
END
```

### Repository Unit Tests

**Location:** Create test project or add to existing

```csharp
public class {EntityName}RepositoryTests
{
    [Fact]
    public async Task GetAllAsync_WithValidSchoolId_ReturnsItems()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_GetAll_{EntityName}")
            .Options;

        using var context = new ApplicationDbContext(options);
        var schoolId = Guid.NewGuid();
        
        context.{EntityNames}.AddRange(
            new {EntityName} { Id = Guid.NewGuid(), SchoolId = schoolId, Name = "Test 1" },
            new {EntityName} { Id = Guid.NewGuid(), SchoolId = schoolId, Name = "Test 2" }
        );
        await context.SaveChangesAsync();

        var repository = new {EntityName}Repository(context);

        // Act
        var result = await repository.GetAllAsync(schoolId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_FromDifferentSchool_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_GetById_{EntityName}")
            .Options;

        using var context = new ApplicationDbContext(options);
        var schoolA = Guid.NewGuid();
        var schoolB = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        
        context.{EntityNames}.Add(
            new {EntityName} { Id = entityId, SchoolId = schoolB, Name = "Test" }
        );
        await context.SaveChangesAsync();

        var repository = new {EntityName}Repository(context);

        // Act
        var result = await repository.GetByIdAsync(entityId, schoolA);

        // Assert
        Assert.Null(result); // Should not return entity from different school
    }
}
```

---

## ✅ Final Checklist

**Entity Model:**
- [ ] Has Guid Id
- [ ] Has Guid SchoolId
- [ ] Has IsActive flag
- [ ] Has audit fields (CreatedAt, UpdatedAt)
- [ ] Navigation properties properly typed

**Configuration:**
- [ ] Max lengths on strings
- [ ] Required fields marked
- [ ] Foreign keys configured with appropriate DeleteBehavior
- [ ] Index on SchoolId
- [ ] Composite index on SchoolId + IsActive
- [ ] Unique constraints consider SchoolId

**Migration:**
- [ ] Creates table with all columns
- [ ] Default values set
- [ ] Foreign keys created
- [ ] Indexes created
- [ ] Down() method implemented
- [ ] Applied to database successfully

**Repository:**
- [ ] All methods have schoolId parameter
- [ ] All queries filter by SchoolId
- [ ] GetAll uses AsNoTracking + OrderBy
- [ ] Delete is soft delete
- [ ] Registered in DI container

**Testing:**
- [ ] Seed data script created (optional)
- [ ] Cross-school access test added
- [ ] GetAll test added

---

## 🚨 Common Mistakes to Avoid

1. **Forgetting SchoolId column** → Data leak vulnerability
2. **No index on SchoolId** → Slow queries
3. **Unique constraints without SchoolId** → Prevents same name in different schools
4. **Hard delete instead of soft delete** → Data loss
5. **Missing Down() in migration** → Can't rollback
6. **Not using AsNoTracking** → 40% slower reads
7. **Not ordering GetAll results** → Inconsistent pagination

---

## ⏱️ Time Breakdown

- Step 1 (Entity Model): 10 min
- Step 2 (Configuration): 10 min
- Step 3 (DbContext): 5 min
- Step 4 (Migration): 10 min
- Step 5 (Repository Interface): 5 min
- Step 6 (Repository Implementation): 10 min
- Step 7 (DI Registration): 5 min
- **Total: 55 minutes**

With practice, you can complete this in **30-35 minutes**.

---

**Next Steps:**
- Follow `add-new-controller.md` to create API endpoints
- Follow `add-new-service.md` to add business logic
- Follow `add-frontend-component.md` to create UI

**Related Files:**
- `.copilot/workflows/add-new-controller.md`
- `.copilot/context/multi-tenancy-pattern.md`
- `.copilot/critical-rules.md`
