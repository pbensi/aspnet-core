using app.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.migrator.Configurations
{
    internal sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
    {
        public void Configure(EntityTypeBuilder<UserNotification> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Property(p => p.SenderGuid)
                .IsRequired();

            builder.Property(p => p.RecieverGuid)
                .IsRequired();

            builder.Property(p => p.Message)
                .HasMaxLength(250);

            builder.Property(p => p.IsSeen)
                .HasDefaultValue(false);

            builder.Property(p => p.IsRead)
                .HasDefaultValue(false);

            builder.Property(p => p.IsDelete)
                .HasDefaultValue(false);

            builder.Property(p => p.SeenAt);
        }
    }
}
