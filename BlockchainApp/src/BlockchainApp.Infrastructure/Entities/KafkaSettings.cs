namespace BlockchainApp.Infrastructure.Entities;

public class KafkaSettings
{
    public string BootstrapServers { get; set; }
    public string Topic { get; set; }
    public string GroupId { get; set; }
}