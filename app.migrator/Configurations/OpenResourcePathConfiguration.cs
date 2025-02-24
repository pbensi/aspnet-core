using app.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.migrator.Configurations
{
    internal sealed class OpenResourcePathConfiguration : IEntityTypeConfiguration<OpenResourcePath>
    {
        public void Configure(EntityTypeBuilder<OpenResourcePath> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Property(p => p.RequestMethod)
                .HasMaxLength(8);

            builder.Property(p => p.RequestPath)
                .HasMaxLength(120);

            builder.Property(p => p.Description)
                .HasMaxLength(120);

            builder.Property(p => p.AllowedRole)
                .HasMaxLength(6);
        }
    }
}
