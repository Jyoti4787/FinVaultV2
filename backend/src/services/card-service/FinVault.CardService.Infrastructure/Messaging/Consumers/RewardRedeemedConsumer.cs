using FinVault.CardService.Infrastructure.Persistence;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinVault.CardService.Infrastructure.Messaging.Consumers;

/// <summary>
/// This consumer listens for reward redemption messages from the Payment Service.
/// When a user redeems points, it adds the cashback amount back as credit to their card.
/// </summary>
public class RewardRedeemedConsumer : IConsumer<IRewardRedeemedMessage>
{
    private readonly CardDbContext _dbContext;
    private readonly ILogger<RewardRedeemedConsumer> _logger;

    public RewardRedeemedConsumer(CardDbContext dbContext, ILogger<RewardRedeemedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IRewardRedeemedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing reward redemption for User: {UserId}, Card: {CardId}, Amount: {Amount}", 
            message.UserId, message.CardId, message.Amount);

        var card = await _dbContext.CreditCards
            .FirstOrDefaultAsync(c => c.Id == message.CardId && c.UserId == message.UserId);

        if (card == null)
        {
            _logger.LogWarning("Card {CardId} not found for User {UserId}. Cannot apply reward credit.", 
                message.CardId, message.UserId);
            return;
        }

        // Apply credit back to the card.
        // Redemption reduces the outstanding balance (making it 'more positive' or 'less negative')
        card.OutstandingBalance -= message.Amount;
        card.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully applied ₹{Amount} credit to Card {CardId}. New Balance: {Balance}", 
            message.Amount, card.Id, card.OutstandingBalance);
    }
}
