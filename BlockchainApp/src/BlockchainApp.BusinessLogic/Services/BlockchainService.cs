using BlockchainApp.Domain.Entities;
using BlockchainApp.Domain.Services;

namespace BlockchainApp.BusinessLogic.Services;

public class BlockchainService(IBlockchainRepository blockchainRepository) : IBlockchainService
{
    public async Task<(IEnumerable<BlockchainData> Data, int TotalCount)> GetBlockchainHistory(int skip, int pageSize, string? blockchainApi)
    {
        return blockchainApi is not null
            ? await blockchainRepository.GetAllByBlockchainApisAsync(blockchainApi, skip, pageSize)
            : await blockchainRepository.GetAllAsync(skip, pageSize);

    }
}