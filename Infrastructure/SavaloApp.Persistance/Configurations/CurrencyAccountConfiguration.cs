using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class CurrencyAccountConfiguration : IEntityTypeConfiguration<CurrencyAccount>
{
    public void Configure(EntityTypeBuilder<CurrencyAccount> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Icon)
            .IsRequired()
            .HasMaxLength(250);
   

      

        builder.HasOne(x => x.User)
            .WithMany(x => x.CurrencyAccounts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Categories)
            .WithOne(x => x.CurrencyAccount)
            .HasForeignKey(x => x.CurrencyAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Goals)
            .WithOne(x => x.CurrencyAccount)
            .HasForeignKey(x => x.CurrencyAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}