using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class CategorySectionConfiguration : IEntityTypeConfiguration<CategorySection>
{
    public void Configure(EntityTypeBuilder<CategorySection> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Icon)
            .IsRequired()
            .HasMaxLength(250);
    }
}