using System.Text.Json;
using BlockchainApp.Domain.Entities;
using BlockchainApp.Domain.Services;
using BlockchainApp.Infrastructure.Entities;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlockchainApp.Infrastructure.Services;

public class KafkaConsumerService(
    ILogger<KafkaConsumerService> logger,
    IOptions<KafkaSettings> kafkaOptions,
    IConsumer<Ignore, string> consumer,
    IServiceProvider serviceProvider)
    : IMessageBrokerConsumer
{
    public async Task ConsumeMessagesAsync(CancellationToken cancellationToken)
    {
        consumer.Subscribe(kafkaOptions.Value.Topic);
        consumer.Assign(new TopicPartition(kafkaOptions.Value.Topic, new Partition(0)));

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume();
                    var message = consumeResult.Message.Value;

                    logger.LogInformation($"Received message: {message}");

                    var enhancedData = JsonSerializer.Deserialize<EnhancedData>(message);
                    if (enhancedData != null)
                    {
                        using var scope = serviceProvider.CreateScope();
                        var blockchainRepository = scope.ServiceProvider.GetRequiredService<IBlockchainRepository>();

                      var blockchainData = new BlockchainData
                      {
                          CreatedAt = enhancedData.CreatedAt,
                          Json = enhancedData.Data,
                          BlockchainApi = enhancedData.BlockchainApi
                      };

                        await blockchainRepository.AddAsync(blockchainData);
                        logger.LogInformation("Message successfully saved to database.");
                    }
                    else
                    {
                        logger.LogWarning("Deserialized data is null.");
                    }
                }
                catch (ConsumeException ex)
                {
                    logger.LogError($"Kafka consume error: {ex.Error.Reason}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error processing message: {ex.Message}");
                }
            }
        }
        finally
        {
            logger.LogInformation("Closing Kafka consumer.");
            consumer.Close();
        }
    }
}
