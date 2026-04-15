using FinVault.IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinVault.IdentityService.Infrastructure.Persistence;

public static class IdentityDataSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityDbContext>>();

        try
        {
            var adminEmail = "admin@finvault.io";
            var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

            if (existingAdmin == null)
            {
                logger.LogInformation("Seeding Admin user: {Email}", adminEmail);

                var admin = new User
                {
                    Id = Guid.Parse("9f3984c4-96a7-4249-b9e2-6f1318ca0a29"),
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                context.Users.Add(admin);
                await context.SaveChangesAsync();
                
                logger.LogInformation("Admin user seeded successfully.");
            }
            else if (existingAdmin.Role != "Admin")
            {
                logger.LogInformation("Updating existing user {Email} to Admin role.", adminEmail);
                existingAdmin.Role = "Admin";
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the Admin user.");
        }
    }
}
