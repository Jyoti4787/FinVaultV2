using FinVault.CardService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinVault.CardService.Infrastructure.Persistence.Configurations;

public class CreditCardConfiguration
    : IEntityTypeConfiguration<CreditCard>
{
    public void Configure(EntityTypeBuilder<CreditCard> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MaskedNumber).IsRequired().HasMaxLength(20);
        builder.Property(x => x.CardholderName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CreditLimit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.OutstandingBalance).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsDeleted })
               .HasDatabaseName("IX_CreditCards_UserId_IsDeleted");
        builder.HasOne(x => x.Issuer)
               .WithMany()
               .HasForeignKey(x => x.IssuerId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable("CreditCards");
    }
}