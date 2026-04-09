using FinVault.CardService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinVault.CardService.Infrastructure.Persistence.Configurations;

public class CardIssuerConfiguration
    : IEntityTypeConfiguration<CardIssuer>
{
    public void Configure(EntityTypeBuilder<CardIssuer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
               .IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.BinPrefixes)
               .IsRequired().HasMaxLength(200);
        builder.ToTable("CardIssuers");

        // Seed the 4 issuers at startup
        // So they are always in the database
        builder.HasData(
            new CardIssuer
            {
                Id          = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name        = "Visa",
                CardLength  = 16,
                BinPrefixes = "4",
                CreatedAt   = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new CardIssuer
            {
                Id          = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name        = "MasterCard",
                CardLength  = 16,
                BinPrefixes = "51,52,53,54,55,2221,2720",
                CreatedAt   = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new CardIssuer
            {
                Id          = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name        = "Amex",
                CardLength  = 15,
                BinPrefixes = "34,37",
                CreatedAt   = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new CardIssuer
            {
                Id          = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name        = "RuPay",
                CardLength  = 16,
                BinPrefixes = "60,65,81,82,508",
                CreatedAt   = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
            });
    }
}