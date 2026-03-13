using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class GoalTransactionConfiguration : IEntityTypeConfiguration<GoalTransaction>
{
    public void Configure(EntityTypeBuilder<GoalTransaction> builder)
    {
        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.TransactionType)
            .IsRequired();

        builder.HasOne(x => x.Goal)
            .WithMany(x => x.GoalTransactions)
            .HasForeignKey(x => x.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}