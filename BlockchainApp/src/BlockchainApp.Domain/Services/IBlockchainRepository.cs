using BlockchainApp.Domain.Entities;

namespace BlockchainApp.Domain.Services
{
    public interface IBlockchainRepository
    {
        Task AddAsync(BlockchainData data);
        Task<(IEnumerable<BlockchainData> Data, int TotalCount)> GetAllAsync(int skip, int pageSize);

        Task<(IEnumerable<BlockchainData> Data, int TotalCount)> GetAllByBlockchainApisAsync(string blockchainApi,
            int skip, int pageSize);
    }
}
