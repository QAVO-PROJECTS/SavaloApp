using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class CategoryTransactionConfiguration : IEntityTypeConfiguration<CategoryTransaction>
{
    public void Configure(EntityTypeBuilder<CategoryTransaction> builder)
    {
        builder.Property(x => x.TransactionName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.TransactionType)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.CategoryTransactions)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}