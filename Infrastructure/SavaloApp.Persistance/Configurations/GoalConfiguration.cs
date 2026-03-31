using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.ColorCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(x => x.CurrencyAccount)
            .WithMany(x => x.Goals)
            .HasForeignKey(x => x.CurrencyAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Icon)
            .WithMany(x => x.Goals)
            .HasForeignKey(x => x.IconId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.GoalSection)
            .WithMany(x => x.Goals)
            .HasForeignKey(x => x.GoalSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.GoalTransactions)
            .WithOne(x => x.Goal)
            .HasForeignKey(x => x.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}