namespace BlockchainApp.Domain.Services;

public interface IMessageBrokerConsumer
{
    Task ConsumeMessagesAsync(CancellationToken cancellationToken);
}