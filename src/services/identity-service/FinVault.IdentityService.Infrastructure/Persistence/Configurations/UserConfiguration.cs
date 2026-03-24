using FinVault.IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinVault.IdentityService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email)
               .IsRequired()
               .HasMaxLength(256);
        builder.HasIndex(x => x.Email)
               .IsUnique();
        builder.Property(x => x.PasswordHash)
               .IsRequired()
               .HasMaxLength(512);
        builder.Property(x => x.FirstName)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(x => x.LastName)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(x => x.Role)
               .IsRequired()
               .HasMaxLength(20);
        builder.ToTable("Users");
    }
}
