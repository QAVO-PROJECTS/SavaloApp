using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class TermsAndConditionConfiguration : IEntityTypeConfiguration<TermsAndCondition>
{
    public void Configure(EntityTypeBuilder<TermsAndCondition> builder)
    {
        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnType("text");
    }
}