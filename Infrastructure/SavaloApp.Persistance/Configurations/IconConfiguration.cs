using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class IconConfiguration : IEntityTypeConfiguration<Icon>
{
    public void Configure(EntityTypeBuilder<Icon> builder)
    {
        builder.Property(x => x.IconName)
            .IsRequired()
            .HasMaxLength(150);
    }
}