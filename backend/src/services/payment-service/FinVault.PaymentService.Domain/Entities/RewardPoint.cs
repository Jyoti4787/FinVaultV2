namespace FinVault.PaymentService.Domain.Entities;

public class RewardPoint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid PaymentId { get; set; }
    public Guid? CardId { get; set; } // The card where redemption was applied (null for earned points)
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Earned"; // Earned | Redeemed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
