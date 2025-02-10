using System;
using System.Threading.Tasks;
using BlockchainApp.BusinessLogic.Services;
using BlockchainApp.Domain.Services;
using BlockchainApp.Infrastructure.Context;
using BlockchainApp.Infrastructure.Entities;
using BlockchainApp.Infrastructure.Repositories;
using BlockchainApp.Infrastructure.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

//var builder = WebApplication.CreateBuilder(args);
//var appMode = Environment.GetEnvironmentVariable("APP_MODE") ?? "api";
//builder.Configuration
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddEnvironmentVariables();

//builder.Logging.ClearProviders();
//builder.Logging.SetMinimumLevel(LogLevel.Trace);
//LogManager.LoadConfiguration("nlog.config");
//builder.Host.UseNLog();

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


//builder.Services.AddScoped<IBlockchainRepository, BlockchainRepository>();
//builder.Services.AddSingleton<IMessageBrokerConsumer, KafkaConsumerService>();
//builder.Services.AddScoped<IBlockchainService, BlockchainService>();
//builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

//builder.Services.AddSingleton<IConsumer<Ignore, string>>(provider =>
//{
//    var kafkaOptions = provider.GetRequiredService<IOptions<KafkaSettings>>().Value;

//    if (string.IsNullOrWhiteSpace(kafkaOptions.BootstrapServers))
//    {
//        throw new ArgumentException("Kafka BootstrapServers is not configured.");
//    }

//    if (string.IsNullOrWhiteSpace(kafkaOptions.GroupId))
//    {
//        throw new ArgumentException("Kafka GroupId is not configured.");
//    }

//    if (string.IsNullOrWhiteSpace(kafkaOptions.Topic))
//    {
//        throw new ArgumentException("Kafka Topic is not configured.");
//    }

//    var consumerConfig = new ConsumerConfig
//    {
//        BootstrapServers = kafkaOptions.BootstrapServers,
//        GroupId = kafkaOptions.GroupId,
//        AutoOffsetReset = AutoOffsetReset.Earliest
//    };

//    return new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
//});


//if (appMode != "api")
//{
//    builder.Services.AddHostedService<BlockchainConsumerService>();
//}

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new() { Title = "BlockchainHistory API", Version = "v1" });
//});

//builder.Services.AddHealthChecks()
//    .AddCheck("self", () => HealthCheckResult.Healthy());

//var app = builder.Build();
//using(var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    dbContext.Database.Migrate();
//}

//app.UseCors("AllowAll");


//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlockchainHistory API V1");
//    });
//}

//app.UseRouting();
//if (appMode == "api")
//{
//    app.MapControllers();
//}

//app.MapHealthChecks("/health");
//app.MapHealthChecks("/status");

//await app.RunAsync();

namespace BlockchainApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var appMode = Environment.GetEnvironmentVariable("APP_MODE") ?? "api";

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        LogManager.LoadConfiguration("nlog.config");
        builder.Host.UseNLog();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IBlockchainRepository, BlockchainRepository>();
        builder.Services.AddSingleton<IMessageBrokerConsumer, KafkaConsumerService>();
        builder.Services.AddScoped<IBlockchainService, BlockchainService>();
        builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

        builder.Services.AddSingleton<IConsumer<Ignore, string>>(provider =>
        {
            var kafkaOptions = provider.GetRequiredService<IOptions<KafkaSettings>>().Value;

            if (string.IsNullOrWhiteSpace(kafkaOptions.BootstrapServers))
            {
                throw new ArgumentException("Kafka BootstrapServers is not configured.");
            }

            if (string.IsNullOrWhiteSpace(kafkaOptions.GroupId))
            {
                throw new ArgumentException("Kafka GroupId is not configured.");
            }

            if (string.IsNullOrWhiteSpace(kafkaOptions.Topic))
            {
                throw new ArgumentException("Kafka Topic is not configured.");
            }

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaOptions.BootstrapServers,
                GroupId = kafkaOptions.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            return new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        });

        if (appMode != "api")
        {
            builder.Services.AddHostedService<BlockchainConsumerService>();
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "BlockchainHistory API", Version = "v1" });
        });

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        app.UseCors("AllowAll");

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
           c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlockchainHistory API V1");
        });

        app.UseRouting();
        if (appMode == "api")
        {
            app.MapControllers();
        }

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/status");

        await app.RunAsync();
    }
}