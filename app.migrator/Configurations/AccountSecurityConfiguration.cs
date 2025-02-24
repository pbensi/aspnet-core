using app.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.migrator.Configurations
{
    internal sealed class AccountSecurityConfiguration : IEntityTypeConfiguration<AccountSecurity>
    {
        public void Configure(EntityTypeBuilder<AccountSecurity> builder)
        {
            builder.HasKey(p => p.UserGuid);

            builder.Property(p => p.UserGuid)
                .IsRequired();

            builder.Property(p => p.EncryptedKey)
                .HasMaxLength(256)
                .IsRequired();

            builder.HasIndex(p => p.EncryptedKey)
                .IsUnique();

            builder.Property(p => p.EncryptedIV)
                .HasMaxLength(256)
                .IsRequired();

            builder.HasIndex(p => p.EncryptedIV)
                .IsUnique();

            builder.Property(p => p.Ipv4)
                .HasMaxLength(45)
                .IsRequired();

            builder.Property(p => p.Ipv6)
                .HasMaxLength(45)
                .IsRequired();

            builder.Property(p => p.OS)
                .IsRequired();

            builder.HasOne(p => p.Account)
                .WithOne(p => p.AccountSecurity)
                .HasForeignKey<AccountSecurity>(p => p.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
