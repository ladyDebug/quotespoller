using DataFetcher.Domain.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using DataFetcher.Domain.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DataFetcher.BusinessLogic.Services;

public class KafkaProducer: IBroker
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IOptions<KafkaOptions> options, ILogger<KafkaProducer> logger)
    {
        _topic = options.Value.Topic;
        _logger = logger;

        var config = new ProducerConfig { BootstrapServers = options.Value.BootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(string key, object data)
    {
        await Task.Delay(1000);
        try
        {
            var jsonData = JsonConvert.SerializeObject(data);

            var message = new Message<string, string>
            {
                Key = key,
                Value = jsonData
            };
            var res = await _producer.ProduceAsync(_topic,  message);

            _logger.LogInformation($"Message was sent to Kafka: {key} | Partition: {res.TopicPartition}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: Sending data to the Kafka: {ex.Message}");
        }
    }
}