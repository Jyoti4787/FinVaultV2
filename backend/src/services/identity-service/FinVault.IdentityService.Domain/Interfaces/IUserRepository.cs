// WHAT IS AN INTERFACE?
// Think of it like a JOB DESCRIPTION
// It says "whoever does this job MUST know how to do these things"
// But it doesn't say HOW to do them
// The actual HOW is written in UserRepository.cs (Infrastructure layer)
//
// WHY do we do this?
// Because Application layer needs to talk to the database
// BUT Application layer is not allowed to know about EF Core or SQL
// So we create this contract in Domain layer
// Application layer uses the contract
// Infrastructure layer fulfills the contract
// They never directly touch each other

using FinVault.IdentityService.Domain.Entities;

namespace FinVault.IdentityService.Domain.Interfaces;

// Published by : Nobody — this is just a contract
// Consumed by  : RegisterUserCommandHandler (ExistsByEmail, Add)
//                LoginUserCommandHandler (GetByEmail)
//                GetUserProfileQueryHandler (GetById)
//                ResetPasswordCommandHandler (GetByEmail, Update)
public interface IUserRepository
{
    // Find a user by their ID
    // Task = this is async (non-blocking)
    // User? = returns User OR null if not found
    // ? means nullable
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);

    // Find a user by their email address
    // Used during login
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);

    // Check if an email is already registered
    // Returns true or false
    // Used during registration to prevent duplicates
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);

    // Save a new user to the database
    // Returns the new user's ID
    Task<Guid> AddAsync(User user, CancellationToken ct);

    // Update an existing user
    // Used when resetting password
    Task UpdateAsync(User user, CancellationToken ct);
}