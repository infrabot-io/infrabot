using Infrabot.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SoftFluent.EntityFrameworkCore.DataEncryption;
using SoftFluent.EntityFrameworkCore.DataEncryption.Providers;
using System.Text;

namespace Infrabot.Common.Contexts
{
    public class InfrabotContext : IdentityDbContext<User>
    {

        private readonly IEncryptionProvider encryptionProvider;
        private static readonly string _encryptionKey = "01234567890123456789012345678901"; // 32-byte key (256-bit key for AES)
        private static readonly string _encryptionIV = "0123456789012345"; // 16-byte IV (128-bit block size for AES)

        public InfrabotContext(DbContextOptions<InfrabotContext> options) : base(options)
        {
            encryptionProvider = new AesProvider(Encoding.UTF8.GetBytes(_encryptionKey), Encoding.UTF8.GetBytes(_encryptionIV));

            var connection = Database.GetDbConnection() as SqliteConnection;
            if (connection != null)
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA journal_mode=WAL;";
                command.ExecuteNonQuery();
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(encryptionProvider);

            // Configure many-to-many relationship between Plugin and Group
            modelBuilder.Entity<GroupPlugin>()
                .HasKey(gp => new { gp.GroupId, gp.PluginId });
            modelBuilder.Entity<GroupPlugin>()
                .HasOne(gp => gp.Group)
                .WithMany(g => g.GroupPlugins)
                .HasForeignKey(gp => gp.GroupId);
            modelBuilder.Entity<GroupPlugin>()
                .HasOne(gp => gp.Plugin)
                .WithMany(p => p.GroupPlugins)
                .HasForeignKey(gp => gp.PluginId);

            // Configure many-to-many relationship between User and Group
            modelBuilder.Entity<UserGroup>()
                .HasKey(ug => new { ug.GroupId, ug.TelegramUserId });
            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.Group)
                .WithMany(g => g.UserGroups)
                .HasForeignKey(ug => ug.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.TelegramUser)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.TelegramUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure realtionship between permission and all objects
            modelBuilder.Entity<PermissionAssignment>()
               .HasMany(pa => pa.Plugins)
               .WithMany(p => p.PermissionAssignments)
               .UsingEntity(j => j.ToTable("PermissionAssignmentPlugins"));

            // (Optional) Configure the many-to-many for TelegramUsers.
            modelBuilder.Entity<PermissionAssignment>()
                .HasMany(pa => pa.TelegramUsers)
                .WithMany() // or use .WithMany(u => u.PermissionAssignments) if added on TelegramUser.
                .UsingEntity(j => j.ToTable("PermissionAssignmentTelegramUsers"));

            // (Optional) Configure the many-to-many for Groups.
            modelBuilder.Entity<PermissionAssignment>()
                .HasMany(pa => pa.Groups)
                .WithMany() // or use .WithMany(g => g.PermissionAssignments) if added on Group.
                .UsingEntity(j => j.ToTable("PermissionAssignmentGroups"));

            modelBuilder.Entity<User>().HasData(
                 new User
                 {
                     Id = Guid.NewGuid().ToString(),
                     Name = "admin",
                     Surname = "aa",
                     UserName = "admin",
                     NormalizedUserName = "ADMIN",
                     Email = "admin@aa.com",
                     NormalizedEmail = "ADMIN@AA.COM",
                     PasswordHash = "AQAAAAIAAYagAAAAEB7N7LlWRtWERCJRKboz8R+Tbe5iupZg6cB0PIIZfP665CpUTH2HaLgwOdLrbnfzyA==",
                     SecurityStamp = "ERGPNXFOAL2U6YZ32OIZL5URPF7O3Q77",
                     ConcurrencyStamp = "3c52c8fb-9450-4b04-9610-7bd7053801d3",
                     CreatedDate = DateTime.Now,
                     UpdatedDate = DateTime.Now,
                     LastLoginDate = DateTime.Now,
                     PhoneNumber = "12313123",
                     IsADIntegrated = false,
                     Enabled = true
                 }
            );

            modelBuilder.Entity<TelegramUser>().HasData(
                 new TelegramUser
                 {
                     Id = 1,
                     Name = "Akshin",
                     Surname = "Mustafayev",
                     TelegramId = 816058261
                 }
            );

            modelBuilder.Entity<Configuration>().HasData(
                new Configuration
                {
                    Id = 1,
                    IsADEnabled = false,
                    ADLogin = "administrator",
                    ADPassword = "A123456a",
                    ADServer = "example.lan",
                    ADDomainName = "example.lan",
                    TelegramBotToken = "asd:asd",
                    TelegramEnableEmergency = true,
                    TelegramEnableShowMyId = true,
                    TelegramPowerShellPath = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
                    TelegramPowerShellArguments = "-ExecutionPolicy Unrestricted -NoProfile",
                    TelegramLinuxShellPath = "/bin/bash",
                    TelegramLinuxShellArguments = "",
                    TelegramPythonPath = "/usr/bin/python",
                    TelegramPythonArguments = "",
                    TelegramResultMaxLength = 12000,
                    UpdatedDate = DateTime.Now
                }
            );

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        public DbSet<TelegramUser> TelegramUsers { get; set; }
        //public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<GroupPlugin> GroupPlugins { get; set; }
        public DbSet<TelegramMessage> TelegramMessages { get; set; }
        public DbSet<PermissionAssignment> PermissionAssignments { get; set; }
        public DbSet<HealthCheck> HealthChecks { get; set; }
    }
}
