using BlockchainApp.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace BlockchainApp.BusinessLogic.Services;

public class BlockchainConsumerService(
    IMessageBrokerConsumer consumerService,
    ILogger<BlockchainConsumerService> logger)
    : IHostedService
{
    private CancellationTokenSource _cts;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka Consumer Service is starting");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task.Run(() => consumerService.ConsumeMessagesAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka Consumer Service is stopping");
        _cts.Cancel();
        return Task.CompletedTask;
    }
}