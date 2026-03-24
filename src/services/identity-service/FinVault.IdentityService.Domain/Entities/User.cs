//This is the simplest file. It just describes what a User looks like in the database. Nothing else.


// This file lives in the Domain layer
// Domain layer has NO knowledge of database, no knowledge of API
// It only knows what a User IS — like a blueprint

namespace FinVault.IdentityService.Domain.Entities;

public class User
{
    // Every user has a unique ID
    // Guid = a big random number like "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    // This is safer than 1,2,3 because nobody can guess the next ID
    public Guid Id { get; set; }

    // The email is used as the login username
    // = string.Empty means it starts as "" instead of null
    // This prevents null reference errors
    public string Email { get; set; } = string.Empty;

    // We NEVER store the real password
    // We store a hashed version — like a scrambled code
    // Even if database is hacked, nobody can read the password
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    // Role decides what the user can access
    // "User" = normal user
    // "Admin" = can see everything
    // "SupportAgent" = can help customers
    // Default is "User" — most people are normal users
    public string Role { get; set; } = "User";

    // Has the user clicked the verification link in their email?
    // false = not verified yet
    // true = verified
    // = false means it starts as false by default
    public bool IsEmailVerified { get; set; } = false;

    // Is the account active?
    // Admin can set this to false to disable someone
    // = true means account is active by default
    public bool IsActive { get; set; } = true;

    // When was this user created?
    // DateTimeOffset stores date + time + timezone info
    // We always store in UTC (universal time) — no confusion between timezones
    public DateTimeOffset CreatedAt { get; set; }

    // When was this user last updated?
    // ? means NULLABLE — it can be null if never updated
    public DateTimeOffset? UpdatedAt { get; set; }
}