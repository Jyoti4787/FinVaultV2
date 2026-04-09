using FinVault.CardService.Domain.Events;
using FinVault.CardService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinVault.CardService.Infrastructure.HostedServices;

public class CardExpiryCheckHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CardExpiryCheckHostedService> _logger;

    public CardExpiryCheckHostedService(
        IServiceProvider serviceProvider,
        ILogger<CardExpiryCheckHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider
                        .GetRequiredService<CardDbContext>();
                    var publishEndpoint = scope.ServiceProvider
                        .GetRequiredService<IPublishEndpoint>();

                    // Check cards expiring in next 30 days
                    var now = DateTime.UtcNow;
                    var thirtyDaysAhead = now.AddDays(30);

                    var expiringCards = await context.CreditCards
                        .Where(c => !c.IsDeleted &&
                                   c.ExpiryYear == thirtyDaysAhead.Year &&
                                   c.ExpiryMonth == thirtyDaysAhead.Month)
                        .ToListAsync(stoppingToken);

                    foreach (var card in expiringCards)
                    {
                        var @event = new CardExpirySoonDomainEvent(
                            card.Id,
                            card.UserId,
                            card.MaskedNumber,
                            card.ExpiryMonth,
                            card.ExpiryYear,
                            Guid.NewGuid());

                        await publishEndpoint.Publish(@event, stoppingToken);
                    }

                    _logger.LogInformation($"Card expiry check completed. Found {expiringCards.Count} cards expiring soon.");
                }

                // Run every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in card expiry check service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}




