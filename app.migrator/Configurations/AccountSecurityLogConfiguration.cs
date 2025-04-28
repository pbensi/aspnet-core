using app.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.migrator.Configurations
{
    internal sealed class AccountSecurityLogConfiguration : IEntityTypeConfiguration<AccountSecurityLog>
    {
        public void Configure(EntityTypeBuilder<AccountSecurityLog> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Property(p => p.UserGuid)
                .IsRequired();

            builder.Property(p => p.OldPublicKey)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(p => p.OldPublicIV)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(p => p.OldPrivateKey)
               .HasMaxLength(256)
               .IsRequired();

            builder.Property(p => p.OldPrivateIV)
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
        }
    }
}
