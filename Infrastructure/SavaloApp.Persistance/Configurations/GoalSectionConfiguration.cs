using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class GoalSectionConfiguration : IEntityTypeConfiguration<GoalSection>
{
    public void Configure(EntityTypeBuilder<GoalSection> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Icon)
            .IsRequired()
            .HasMaxLength(250);
    }
}