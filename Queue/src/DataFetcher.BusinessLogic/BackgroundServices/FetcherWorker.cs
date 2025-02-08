using System.Collections.Concurrent;
using DataFetcher.Domain.Entities;
using DataFetcher.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataFetcher.BusinessLogic.BackgroundServices;

public class FetcherWorker(
    IBlockchainFetcher blockchainFetcher,
    IBroker broker,
    IOptions<BlockchainApiOptions> apiOptions,
    ILogger<FetcherWorker> logger,
    int maxRequestsPerSecond,
    int maxRequestsPerHr)
    : BackgroundService
{
    private ConcurrentQueue<string> _queueApi = new();
    public Task TestExecuteAsync(CancellationToken stoppingToken) => ExecuteAsync(stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiList = apiOptions.Value.Apis.ToList();
        while (_queueApi.Count < 50)
        {
            foreach (var api in apiList.Where(api => _queueApi.Count < 50))
            {
                _queueApi.Enqueue(api.Key);
            }
        }
        var semaphore = new SemaphoreSlim(maxRequestsPerSecond);
        while (!stoppingToken.IsCancellationRequested)
        {
            var itemsToProcess = new List<string>();
            for (var i = 0; i < maxRequestsPerSecond && _queueApi.TryDequeue(out var item); i++)
            {
                itemsToProcess.Add(item);
            }

            if (itemsToProcess.Count == 0)
            {
                Console.WriteLine("Queue is empty");
                break; 
            }

            var tasks = itemsToProcess.Select(async item =>
            {
                await semaphore.WaitAsync(); 
                try
                {
                    var createdAt = DateTime.UtcNow;
                    var blockchain = apiList.FirstOrDefault(x => x.Key == item);
                    var res = await blockchainFetcher.FetchBlockchainDataAsync(blockchain.Value);
                    logger.LogInformation($"Request to {blockchain.Key} completed at {createdAt}");
                    if (!string.IsNullOrEmpty(res))
                    {
                        var enhancedData = new
                        {
                            Data = res,
                            //  Data = "{\"test\":\"test\"}",
                            CreatedAt = createdAt,
                            BlockchainApi = blockchain.Key
                        };

                        await broker.PublishAsync(blockchain.Key, enhancedData);

                        logger.LogInformation(
                            $"Request to {blockchain.Key} completed at {createdAt} with message ");
                    }

                    _queueApi.Enqueue(item);
                }
                finally
                {
                    semaphore.Release(); 
                }
            }).ToList();


            await Task.WhenAll(tasks);

            await Task.Delay((3600000 * maxRequestsPerSecond )/ maxRequestsPerHr , stoppingToken);
        }
    }
}