using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Repeat)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.ColorCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(x => x.CurrencyAccount)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.CurrencyAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Icon)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.IconId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CategorySection)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.CategorySectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CategoryTransactions)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}