using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .IsRequired(false)
            
            .HasMaxLength(100);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.TimeZone)
            .HasMaxLength(100);

        builder.Property(x => x.Language)
            .HasMaxLength(20);

        builder.Property(x => x.AccountType)
            .HasDefaultValue("basic")
            .HasMaxLength(30);
        

        builder.Property(x => x.ProfileImage)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Email)
            .IsUnique(false);



        builder.HasMany(x => x.Reviews)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}