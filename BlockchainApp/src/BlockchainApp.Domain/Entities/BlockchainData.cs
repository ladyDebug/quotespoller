namespace BlockchainApp.Domain.Entities;

public class BlockchainData
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Json { get; set; }
    public string BlockchainApi { get; set; }
}