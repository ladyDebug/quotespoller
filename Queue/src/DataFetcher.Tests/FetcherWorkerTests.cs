using DataFetcher.BusinessLogic.BackgroundServices;
using DataFetcher.Domain.Entities;
using DataFetcher.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Range = Moq.Range;

namespace DataFetcher.Tests;

public class FetcherWorkerTests
{
    private readonly Mock<IBlockchainFetcher> _blockchainFetcherMock;
    private readonly Mock<IBroker> _brokerMock;
    private readonly Mock<IOptions<BlockchainApiOptions>> _apiOptionsMock;
    private readonly Mock<ILogger<FetcherWorker>> _loggerMock;
    private readonly FetcherWorker _fetcherWorker;

    public FetcherWorkerTests()
    {
        _blockchainFetcherMock = new Mock<IBlockchainFetcher>();
        _brokerMock = new Mock<IBroker>();
        _apiOptionsMock = new Mock<IOptions<BlockchainApiOptions>>();
        _loggerMock = new Mock<ILogger<FetcherWorker>>();

        var apiOptions = new BlockchainApiOptions
        {
            Apis = new Dictionary<string, string>
            {
                { "BTC", "https://api.blockcypher.com/v1/btc/main" },
                { "ETH", "https://api.blockcypher.com/v1/eth/main" },
                { "LTC", "https://api.blockcypher.com/v1/ltc/main" },
                { "XRP", "https://api.example.com/xrp" },
                { "DOGE", "https://api.example.com/doge" }
            }
        };

        _apiOptionsMock.Setup(x => x.Value).Returns(apiOptions);

        _fetcherWorker = new FetcherWorker(
            _blockchainFetcherMock.Object,
            _brokerMock.Object,
            _apiOptionsMock.Object,
            _loggerMock.Object,
            3, 100);
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesApisInTwoSecond()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        _blockchainFetcherMock
            .Setup(x => x.FetchBlockchainDataAsync(It.IsAny<string>()))
            .ReturnsAsync("test-data");

        var startTime = DateTime.UtcNow;

        try
        {
            var task = _fetcherWorker.TestExecuteAsync(cancellationTokenSource.Token);

            await Task.Delay(2000);

            cancellationTokenSource.Cancel();

            await task;
        }
        catch (TaskCanceledException)
        {
            _loggerMock.Object.LogWarning("Test execution was cancelled gracefully.");
        }

        var endTime = DateTime.UtcNow;
        var elapsedTime = endTime - startTime;

        _blockchainFetcherMock.Verify(x => x.FetchBlockchainDataAsync(It.IsAny<string>()), Times.Between(1, 6, Range.Inclusive));
        _brokerMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Between(1, 6, Range.Inclusive));

        Assert.InRange(elapsedTime.TotalSeconds, 0, 2.1);

        _loggerMock.Object.LogInformation($"Test completed in {elapsedTime.TotalSeconds} seconds.");
    }
}