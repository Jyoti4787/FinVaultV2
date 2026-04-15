using System;

namespace FinVault.Shared.Contracts.Messages;

/// <summary>
/// This message is sent when a user redeems their reward points for cashback/credit.
/// The Card Service listens to this to add money (credit) back to the user's card.
/// </summary>
public interface IRewardRedeemedMessage
{
    // The user who redeemed the points
    Guid UserId { get; }

    // The credit card that should receive the cashback
    Guid CardId { get; }

    // The amount of money (in INR) to be credited back
    decimal Amount { get; }

    // When the redemption occurred
    DateTime RedeemedAt { get; }
}

public class RewardRedeemedMessage : IRewardRedeemedMessage
{
    public Guid UserId { get; set; }
    public Guid CardId { get; set; }
    public decimal Amount { get; set; }
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
}
