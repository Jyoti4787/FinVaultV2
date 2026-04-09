using FinVault.CardService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinVault.CardService.Infrastructure.Persistence;

public class CardDbContext : DbContext
{
    public CardDbContext(
        DbContextOptions<CardDbContext> options)
        : base(options) { }

    public DbSet<CreditCard> CreditCards => Set<CreditCard>();
    public DbSet<CardIssuer> CardIssuers => Set<CardIssuer>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CardDbContext).Assembly);
    }
}