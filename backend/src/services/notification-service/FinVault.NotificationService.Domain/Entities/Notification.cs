// ==================================================================
// FILE : Notification.cs
// LAYER: Domain (Entities)
// PATH : notification-service/FinVault.NotificationService.Domain/Entities/
//
// WHAT IS THIS?
// This is the entity that represents a USER ALERT.
// Whenever something important happens (New Card, Payment Done, Bill Ready),
// we create one of these to show to the user.
//
// Published by : notification-service (triggered by global events)
// Consumed by  : Angular app via NotificationsController
// ==================================================================

using System;

namespace FinVault.NotificationService.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }

    // Who should see this alert?
    public Guid UserId { get; set; }

    // The actual text message (e.g., "Your payment of ₹5,000 was successful!")
    public string Message { get; set; }

    // Type of alert: "Card", "Payment", "Billing", "System"
    public string Type { get; set; }

    // When did this alert happen?
    public DateTime SentDate { get; set; }

    // Has the user seen it yet? 
    // We start as "false" and change to "true" when they click it
    public bool IsRead { get; set; }

    // Optional: Link to go to (e.g., link to the bill or transaction)
    public string? ActionUrl { get; set; }
}
