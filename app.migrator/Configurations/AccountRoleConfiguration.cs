using app.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.migrator.Configurations
{
    internal sealed class AccountRoleConfiguration : IEntityTypeConfiguration<AccountRole>
    {
        public void Configure(EntityTypeBuilder<AccountRole> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Property(p => p.PageName)
                .HasMaxLength(50);

            builder.Property(p => p.LabelName)
                .HasMaxLength(50);

            builder.Property(p => p.IsPost);
            builder.Property(p => p.IsPut);
            builder.Property(p => p.IsDelete);
            builder.Property(p => p.IsGet);
            builder.Property(p => p.IsOptions);

            builder.HasOne(p => p.Account)
                .WithMany(p => p.AccountRole)
                .HasForeignKey(p => p.UserGuid)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
