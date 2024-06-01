using api.entities;
using Microsoft.EntityFrameworkCore;

namespace api.migrator
{
    public class APIDBContext : DbContext
    {
        public APIDBContext(DbContextOptions<APIDBContext> options) : base(options)
        {
          
        }

        public DbSet<User> User { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(APIDBContext).Assembly);
    }
}
