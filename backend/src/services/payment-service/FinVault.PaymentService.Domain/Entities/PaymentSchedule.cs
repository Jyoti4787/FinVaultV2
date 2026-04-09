namespace FinVault.PaymentService.Domain.Entities;

public class PaymentSchedule
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BillId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled, Failed
    public DateTime? ExecutedAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Bill? Bill { get; set; }
}
