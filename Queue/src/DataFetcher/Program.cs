using DataFetcher.BusinessLogic.BackgroundServices;
using DataFetcher.BusinessLogic.Services;
using DataFetcher.Domain.Entities;
using DataFetcher.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

        LogManager.LoadConfiguration("nlog.config");
        logging.AddNLog(); })
    .ConfigureServices((hostContext, services) =>
    {
        int interval = hostContext.Configuration.GetValue<int>("FetchIntervalSeconds");

        services.AddSingleton<IBlockchainFetcher, BlockchainFetcher>(); 
        services.AddSingleton<IBroker, KafkaProducer>(); 

        services.Configure<KafkaOptions>(hostContext.Configuration.GetSection("Kafka"));
        services.Configure<BlockchainApiOptions>(hostContext.Configuration.GetSection("BlockchainApis"));

        services.AddHttpClient();

        services.AddHostedService(sp =>
            new FetcherWorker(
                sp.GetRequiredService<IBlockchainFetcher>(),
                sp.GetRequiredService<IBroker>(),
                sp.GetRequiredService<IOptions<BlockchainApiOptions>>(),
                sp.GetRequiredService<ILogger<FetcherWorker>>(),
                hostContext.Configuration.GetValue<int>("MaxRequestsPerSecond"),
                hostContext.Configuration.GetValue<int>("MaxRequestsPerHr")

            ));

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());
    })
    .UseNLog()  
    .Build();
await builder.RunAsync();
