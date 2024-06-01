using api.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.migrator.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Property(u => u.Name)
                .HasMaxLength(20);

            builder.Property(u => u.Email)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(u => u.AccountName)
                .HasMaxLength(20);

            builder.Property(u => u.Password)
                .HasMaxLength(20);

            builder.HasIndex(u => u.AccountName)
                .IsUnique();

            var adminUserId = 1;

            builder.HasData(
                new User
                {
                    Id = adminUserId,
                    Name = "Admin",
                    Email = "Admin@example.com",
                    AccountName = "admin",
                    Password = "123qwe",
                    IsActive = true,
                    IsAdmin = true,
                    CreatedBy = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedBy = "Admin",
                    LastModifiedAt = DateTime.UtcNow
                }
            );

            UserRolesConfiguration.AdminUserId = adminUserId;
        }
    }

    internal sealed class UserRolesConfiguration : IEntityTypeConfiguration<UserRoles>
    {
        public static int AdminUserId { get; set; }

        public void Configure(EntityTypeBuilder<UserRoles> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Property(ur => ur.Name)
                .HasMaxLength(50);

            builder.HasIndex(ur => ur.Name)
                .IsUnique();

            builder.HasData(
                new UserRoles
                {
                    Id = 1,
                    Name = "Pages.User.And.Role",
                    CanAdd = true,
                    CanUpdate = true,
                    CanRemove = true,
                    CanView = true,
                    UserId = AdminUserId,
                }
            );
        }
    }
}
