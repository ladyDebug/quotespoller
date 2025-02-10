using BlockchainApp.Domain.Entities;
using BlockchainApp.Domain.Services;
using BlockchainApp.Infrastructure.Entities;
using BlockchainApp.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace BlockchainApp.Tests
{
    public class KafkaConsumerServiceTests
    {
        private readonly Mock<ILogger<KafkaConsumerService>> _loggerMock;
        private readonly Mock<IBlockchainRepository> _blockchainRepositoryMock;
        private readonly Mock<IConsumer<Ignore, string>> _consumerMock;
        private readonly KafkaConsumerService _kafkaConsumerService;

        public KafkaConsumerServiceTests()
        {
            _loggerMock = new Mock<ILogger<KafkaConsumerService>>();
            Mock<IOptions<KafkaSettings>> kafkaOptionsMock = new();
            _blockchainRepositoryMock = new Mock<IBlockchainRepository>();
            _consumerMock = new Mock<IConsumer<Ignore, string>>();
            Mock<IServiceProvider> serviceProviderMock = new();
            Mock<IServiceScope> serviceScopeMock = new();
            Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();

            var kafkaSettings = new KafkaSettings
            {
                BootstrapServers = "localhost:9092",
                GroupId = "test-group",
                Topic = "test-topic"
            };

            kafkaOptionsMock.Setup(x => x.Value).Returns(kafkaSettings);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IBlockchainRepository)))
                .Returns(_blockchainRepositoryMock.Object);

            _kafkaConsumerService = new KafkaConsumerService(
                _loggerMock.Object,
                kafkaOptionsMock.Object,
                _consumerMock.Object,
                serviceProviderMock.Object 
            );
        }

        [Fact]
        public async Task ConsumeMessagesAsync_ShouldProcessMessageAndSaveToRepository()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            var cancellationToken = cts.Token;

            var enhancedData = new EnhancedData
            {
                CreatedAt = DateTime.UtcNow,
                Data = "{\\\"Field1\\\":\\\"value1\\\",\\\"Field2\\\":\\\"value2\\\"}",
                BlockchainApi = "ETH"
            };

            var message = JsonSerializer.Serialize(enhancedData);

            var consumeResult = new ConsumeResult<Ignore, string>
            {
                Message = new Message<Ignore, string> { Value = message }
            };

            _consumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(consumeResult);

            var task = Task.Run(() => _kafkaConsumerService.ConsumeMessagesAsync(cancellationToken));

            await Task.Delay(60);
            cts.Cancel();
            await task;

            _blockchainRepositoryMock.Verify(x => x.AddAsync(It.Is<BlockchainData>(
                data => data.BlockchainApi == "ETH" &&
                        data.Json.Contains("Field1") &&
                        data.CreatedAt == enhancedData.CreatedAt
            )), Times.AtLeastOnce);

        }

        [Fact]
        public async Task ConsumeMessagesAsync_ShouldLogErrorWhenDeserializationFails()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            var cancellationToken = cts.Token;
            var invalidMessage = "null";

            var consumeResult = new ConsumeResult<Ignore, string>
            {
                Message = new Message<Ignore, string> { Value = invalidMessage }
            };

            _consumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(consumeResult);

            var task = Task.Run(() => _kafkaConsumerService.ConsumeMessagesAsync(cancellationToken));
            
            await Task.Delay(60);
            cts.Cancel(); 
            await task;

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deserialized data is null.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ), Times.AtLeastOnce);
        }
        
    }
}
