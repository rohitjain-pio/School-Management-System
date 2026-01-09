using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SMSDataModel.Model.Models;

namespace SMSDataContext.Data
{
    public class DataContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>

    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<Parents> Parents { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<SchoolClass> Classes { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Teacher> TeacherSubject { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        
        // Security-related DbSets
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Attendance>()
                .Property(a => a.Status)
                .HasConversion<string>();

            builder.Entity<Student>()
                .Property(s => s.Gender)
                .HasConversion<string>();

            // ✅ One-to-many: School → Users
            builder.Entity<School>()
                .HasMany(s => s.Users)
                .WithOne(u => u.School)
                .HasForeignKey(u => u.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            // Security entities configuration
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            builder.Entity<PasswordResetToken>()
                .HasOne(prt => prt.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(prt => prt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PasswordResetToken>()
                .HasIndex(prt => prt.Token)
                .IsUnique();

            builder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AuditLog>()
                .HasIndex(al => al.Timestamp);

            builder.Entity<AuditLog>()
                .HasIndex(al => new { al.UserId, al.Action, al.Timestamp });

            //// If you want School delete to NOT remove students automatically
            //builder.Entity<Student>()
            //    .HasOne(s => s.School)
            //    .WithMany()
            //    .HasForeignKey(s => s.SchoolId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //// If you want Class delete to remove Students
            //builder.Entity<Student>()
            //    .HasOne(s => s.Class)
            //    .WithMany()
            //    .HasForeignKey(s => s.ClassId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
