using app.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.migrator.Configurations
{
    internal sealed class PersonalDetailConfiguration : IEntityTypeConfiguration<PersonalDetail>
    {
        public void Configure(EntityTypeBuilder<PersonalDetail> builder)
        {
            builder.HasKey(p => p.UserGuid);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.Id)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder.Property(p => p.UserGuid)
                .IsRequired();

            builder.HasIndex(p => p.UserGuid)
                .IsUnique();

            builder.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.MiddleName)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.Gender)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(p => p.BirthDate)
                .IsRequired();

            builder.Property(p => p.Email)
                .HasMaxLength(256);

            builder.Property(p => p.IsDelete)
                .HasDefaultValue(false);

            builder.HasOne(p => p.Account)
                .WithOne(p => p.PersonalDetail)
                .HasForeignKey<Account>(p => p.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
