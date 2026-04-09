// ==================================================================
// FILE : NotificationRepository.cs
// LAYER: Infrastructure (Persistence)
// PATH : notification-service/FinVault.NotificationService.Infrastructure/Persistence/Repositories/
// ==================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinVault.NotificationService.Domain.Entities;
using FinVault.NotificationService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVault.NotificationService.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.SentDate)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllForUserAsync(Guid userId)
    {
        var alerts = await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();
        
        _context.Notifications.RemoveRange(alerts);
        await _context.SaveChangesAsync();
    }
}
