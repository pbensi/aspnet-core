using app.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.migrator.Configurations
{
    internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(x => x.UserGuid);

            builder.Property(p => p.UserGuid)
                .IsRequired();

            builder.HasIndex(p => p.UserGuid)
                .IsUnique();

            builder.Property(p => p.UserName)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(p => p.UserName)
                .IsUnique();

            builder.Property(p => p.Password)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(false);

            builder.Property(p => p.IsAdmin)
                .HasDefaultValue(false);

            builder.HasOne(p => p.PersonalDetail)
                .WithOne(p => p.Account)
                .HasForeignKey<Account>(p => p.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.AccountSecurity)
                .WithOne(p => p.Account)
                .HasForeignKey<AccountSecurity>(p => p.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.AccountRole)
                .WithOne(p => p.Account)
                .HasForeignKey(p => p.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
