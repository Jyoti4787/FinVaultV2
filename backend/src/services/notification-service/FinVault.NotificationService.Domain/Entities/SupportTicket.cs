using System;

namespace FinVault.NotificationService.Domain.Entities;

public class SupportTicket
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open, InProgress, Resolved
    public string? AdminComment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static SupportTicket Create(Guid userId, string subject, string message)
    {
        return new SupportTicket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Subject = subject,
            Message = message,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Resolve(string comment)
    {
        Status = "Resolved";
        AdminComment = comment;
        UpdatedAt = DateTime.UtcNow;
    }
}
