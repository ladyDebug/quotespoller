using BlockchainApp.Domain.Entities;

namespace BlockchainApp.Domain.Services;

public interface IBlockchainService
{
    Task<(IEnumerable<BlockchainData> Data, int TotalCount)> GetBlockchainHistory(int skip, int pageSize, string? blockchainApi);

}