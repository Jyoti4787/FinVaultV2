// ==================================================================
// FILE : PaymentRepository.cs
// LAYER: Infrastructure (Persistence)
// PATH : payment-service/FinVault.PaymentService.Infrastructure/Persistence/Repositories/
//
// WHAT IS THIS?
// This is the "ACTUAL WORKER" that does the database chores.
// It IMPLEMENTS the IPaymentRepository interface we defined in Domain.
//
// It uses the DbContext (the remote control) to talk to SQL Server.
//
// Published by : Infrastructure Layer
// Consumed by  : Application Layer (Handlers) via the Interface
// ==================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinVault.PaymentService.Domain.Entities;
using FinVault.PaymentService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVault.PaymentService.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    // Save a new payment record
    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    // Find a payment by its Guid ID
    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Get all payments for a user (ordered by newest first)
    public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync();
    }

    // Update an existing payment (Status change, etc.)
    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }

    // Prevent duplicate payments!
    public async Task<bool> ExistsByExternalIdAsync(string externalId)
    {
        return await _context.Payments
            .AnyAsync(p => p.ExternalTransactionId == externalId);
    }
}
