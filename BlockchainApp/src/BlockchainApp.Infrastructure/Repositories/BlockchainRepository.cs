using BlockchainApp.Domain.Entities;
using BlockchainApp.Domain.Services;
using BlockchainApp.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BlockchainApp.Infrastructure.Repositories
{
    public class BlockchainRepository(AppDbContext context) : IBlockchainRepository
    {
        public async Task AddAsync(BlockchainData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            await context.BlockchainData.AddAsync(data);
            await context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<BlockchainData> Data, int TotalCount)> GetAllAsync(int skip, int pageSize)
        {
            var query = context.BlockchainData.OrderByDescending(x => x.CreatedAt);

            var totalCount = await query.CountAsync();
            var data = await query.Skip(skip  * pageSize).Take(pageSize).ToListAsync();

            return (data, totalCount);
        }

        public async Task<(IEnumerable<BlockchainData> Data, int TotalCount)> GetAllByBlockchainApisAsync(string blockchainApi, int skip, int pageSize)
        {
            var query = context.BlockchainData
                .Where(x => x.BlockchainApi == blockchainApi)
                .OrderByDescending(x => x.CreatedAt);

            var totalCount = await query.CountAsync();
            var data = await query.Skip(skip * pageSize).Take(pageSize).ToListAsync();

            return (data, totalCount);
        }
    }
}
