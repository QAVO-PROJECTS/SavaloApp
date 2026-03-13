using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Context;

public class SavaloAppDbContext:IdentityDbContext<User>
{
    public SavaloAppDbContext(DbContextOptions<SavaloAppDbContext> options) : base(options)
    {
    }

    public DbSet<CurrencyAccount> CurrencyAccounts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<CategoryTransaction> CategoryTransactions { get; set; }
    public DbSet<GoalTransaction> GoalTransactions { get; set; }
    public DbSet<Icon> Icons { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<GoalSection> GoalSections { get; set; }
    public DbSet<CategorySection> CategorySections { get; set; }
    public DbSet<TermsAndCondition> TermsAndConditions { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(SavaloAppDbContext).Assembly);
    } 
}