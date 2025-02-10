using DataFetcher.BusinessLogic.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace DataFetcher.Tests;

public class BlockchainFetcherTests
{
    [Fact]
    public async Task FetchBlockchainData_ShouldReturnEmptyString_WhenRequestTimesOut()
    {
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();

        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected() 
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TaskCanceledException("The operation was canceled."));  

        var httpClient = new HttpClient(mockHandler.Object);

       mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        
        var mockLogger = new Mock<ILogger<BlockchainFetcher>>();


        var blockchainFetcher = new BlockchainFetcher(mockHttpClientFactory.Object, mockLogger.Object);
        var result = await blockchainFetcher.FetchBlockchainDataAsync("https://invalid-url.com");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task FetchBlockchainData_ShouldReturnEmptyString_WhenApiCallFails()
    {
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();

        
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network unavailable"));

        var httpClient = new HttpClient(mockHandler.Object);

        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        var mockLogger = new Mock<ILogger<BlockchainFetcher>>();

        var blockchainFetcher = new BlockchainFetcher(mockHttpClientFactory.Object, mockLogger.Object);
        var result = await blockchainFetcher.FetchBlockchainDataAsync("https://invalid-url.com");
        Assert.Equal(string.Empty, result);
    }
}