// ==================================================================
// FILE : INotificationRepository.cs
// LAYER: Domain (Interfaces)
// PATH : notification-service/FinVault.NotificationService.Domain/Interfaces/
//
// WHAT IS THIS?
// The "Book of Rules" for the Notification database.
//
// Published by : Domain Layer
// Consumed by  : Application Layer (Handlers)
// Implemented by: Infrastructure Layer (Repositories)
// ==================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinVault.NotificationService.Domain.Entities;

namespace FinVault.NotificationService.Domain.Interfaces;

public interface INotificationRepository
{
    // Get all notifications for a specific user
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);

    // Save a new notification
    Task AddAsync(Notification notification);

    // Mark a notification as read (so it stops "glowing" on the UI)
    Task MarkAsReadAsync(Guid id);

    // Clear all notifications for a user
    Task DeleteAllForUserAsync(Guid userId);
}
