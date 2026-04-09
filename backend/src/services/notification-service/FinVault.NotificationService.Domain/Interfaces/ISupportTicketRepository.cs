using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinVault.NotificationService.Domain.Entities;

namespace FinVault.NotificationService.Domain.Interfaces;

public interface ISupportTicketRepository
{
    Task<SupportTicket?> GetByIdAsync(Guid id);
    Task<IEnumerable<SupportTicket>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<SupportTicket>> GetAllAsync();
    Task AddAsync(SupportTicket ticket);
    Task UpdateAsync(SupportTicket ticket);
}
