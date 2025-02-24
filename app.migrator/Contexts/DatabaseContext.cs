using app.entities;
using Microsoft.EntityFrameworkCore;

namespace app.migrator.Contexts
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountRole> AccountRoles { get; set; }
        public DbSet<AccountSecurity> AccountSecurities { get; set; }
        public DbSet<AccountSecurityLog> AccountSecurityLogs { get; set; }
        public DbSet<OpenResourcePath> OpenResourcePaths { get; set; }
        public DbSet<PersonalDetail> PersonalDetails { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }
}
