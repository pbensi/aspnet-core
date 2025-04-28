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

            builder.Property(p => p.PublicKey)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(p => p.PublicIV)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(p => p.PrivateKey)
               .HasMaxLength(256)
               .IsRequired();

            builder.Property(p => p.PrivateIV)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(p => p.DeviceName)
                .HasMaxLength(255)
                .IsRequired(false);

            builder.Property(p => p.Ipv4Address)
                .HasMaxLength(45)
                .IsRequired(false);

            builder.Property(p => p.Ipv6Address)
                .HasMaxLength(45)
                .IsRequired(false);

            builder.Property(p => p.OperatingSystem)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.HasOne(p => p.Account)
                .WithOne(p => p.AccountSecurity)
                .HasForeignKey<AccountSecurity>(p => p.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
