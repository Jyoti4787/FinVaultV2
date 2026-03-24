using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVault.IdentityService.Infrastructure.Persistence.Repositories;

// WHAT IS A REPOSITORY?
// Remember the Interface (IUserRepository) in Domain layer?
// It was just a CONTRACT — it said WHAT to do
// This class is the actual WORKER — it says HOW to do it
//
// Domain layer: "I need someone who can find a user by email"
// This class:   "OK here is exactly how I do that using EF Core"
//
// WHY this pattern?
// Application layer only knows IUserRepository
// It does not know EF Core exists
// You could swap SQL Server for MongoDB tomorrow
// Application layer code would NOT change at all
// Only this file changes

// Published by : Program.cs registers this as IUserRepository
// Consumed by  : All command and query handlers
public class UserRepository : IUserRepository
{
    // IdentityDbContext = our database connection
    private readonly IdentityDbContext _ctx;

    // Constructor injection
    // .NET automatically gives us the DbContext
    public UserRepository(IdentityDbContext ctx)
        => _ctx = ctx;

    // Find user by ID
    // FindAsync is EF Core's fastest way to find by primary key
    // Returns null if not found
    // [id] is an array because FindAsync takes params array
    public async Task<User?> GetByIdAsync(
        Guid id, CancellationToken ct)
        => await _ctx.Users.FindAsync([id], ct);

    // Find user by email
    // FirstOrDefaultAsync = return first match or null
    // We search by exact email match
    public async Task<User?> GetByEmailAsync(
        string email, CancellationToken ct)
        => await _ctx.Users
            .FirstOrDefaultAsync(
                x => x.Email == email, ct);

    // Check if email exists
    // AnyAsync = returns true if ANY row matches
    // Faster than fetching the full user object
    // We use this during registration check
    public async Task<bool> ExistsByEmailAsync(
        string email, CancellationToken ct)
        => await _ctx.Users
            .AnyAsync(x => x.Email == email, ct);

    // Save new user to database
    // AddAsync = adds to the in-memory tracker
    // SaveChangesAsync = actually runs INSERT SQL
    // Returns the new user's ID
    public async Task<Guid> AddAsync(
        User user, CancellationToken ct)
    {
        await _ctx.Users.AddAsync(user, ct);
        await _ctx.SaveChangesAsync(ct);
        return user.Id;
    }

    // Update existing user
    // Update = marks all properties as modified
    // SaveChangesAsync = runs UPDATE SQL
    public async Task UpdateAsync(
        User user, CancellationToken ct)
    {
        _ctx.Users.Update(user);
        await _ctx.SaveChangesAsync(ct);
    }
}