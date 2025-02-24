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

            builder.Property(p => p.OldEncryptedKey)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(p => p.OldEncryptedIV)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(p => p.Ipv4)
                .HasMaxLength(45)
                .IsRequired();

            builder.Property(p => p.Ipv6)
                .HasMaxLength(45)
                .IsRequired();

            builder.Property(p => p.OS)
                .IsRequired();
        }
    }
}
