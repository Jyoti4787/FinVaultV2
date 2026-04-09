// ==================================================================
// FILE : IPaymentRepository.cs
// LAYER: Domain (Interfaces)
// PATH : payment-service/FinVault.PaymentService.Domain/Interfaces/
//
// WHAT IS THIS?
// This is a "BOOK OF RULES" (Contract) for our database.
// It tells the rest of the application: 
// "I don't care HOW you talk to SQL Server, but you MUST be able 
// to do these 5 things for Payments!"
//
// Why use an interface?
// Because if we want to change from SQL Server to MongoDB later,
// we just write a new "implementation" and the business logic
// doesn't have to change!
//
// Published by : Domain Layer
// Consumed by  : Application Layer (Handlers)
// Implemented by: Infrastructure Layer (Repositories)
// ==================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinVault.PaymentService.Domain.Entities;

namespace FinVault.PaymentService.Domain.Interfaces;

public interface IPaymentRepository
{
    // Find one payment by its ID
    Task<Payment?> GetByIdAsync(Guid id);

    // Get all payments for a specific user (History)
    Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId);

    // Save a NEW payment receipt to the database
    Task AddAsync(Payment payment);

    // Update an existing payment (e.g., change from Pending to Success)
    Task UpdateAsync(Payment payment);

    // Check if a payment with a specific external ID already exists
    // (Prevents charging the user twice if they click button fast!)
    Task<bool> ExistsByExternalIdAsync(string externalId);
}
