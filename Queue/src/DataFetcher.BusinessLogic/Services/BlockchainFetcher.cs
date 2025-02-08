using Microsoft.Extensions.Logging;
using DataFetcher.Domain.Services;

namespace DataFetcher.BusinessLogic.Services;

public class BlockchainFetcher(
    IHttpClientFactory httpClientFactory,
    ILogger<BlockchainFetcher> logger)
    : IBlockchainFetcher
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    public async Task<string> FetchBlockchainDataAsync(string apiUrl)
    {
        try
        {
            logger.LogInformation($"Sending request to API {apiUrl}...");
            var response = await _httpClient.GetStringAsync(apiUrl);
            return response;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError($"Request to {apiUrl} timed out: {ex.Message}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error fetching data from {apiUrl}: {ex.Message}");
            return string.Empty;
        }
    }

}