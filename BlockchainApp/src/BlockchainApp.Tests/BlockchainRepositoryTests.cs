using BlockchainApp.Domain.Entities;
using BlockchainApp.Domain.Services;
using BlockchainApp.Infrastructure.Context;
using BlockchainApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlockchainApp.Tests
{
    public class BlockchainRepositoryTests: IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IBlockchainRepository _repository;

        public BlockchainRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new BlockchainRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddDataToDatabase()
        {
            
            var blockchainData = new BlockchainData
            {
                CreatedAt = DateTime.UtcNow,
                Json = "{\"key\": \"value\"}",
                BlockchainApi = "ETH"
            };

            await _repository.AddAsync(blockchainData);
            var result = await _context.BlockchainData.FirstOrDefaultAsync();
            
            Assert.NotNull(result);
            Assert.Equal("ETH", result.BlockchainApi);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedData()
        {
            
            var mockData = new List<BlockchainData>
            {
                new() { CreatedAt = DateTime.UtcNow.AddDays(-1), BlockchainApi = "BTC", Json = "{}" },
                new() { CreatedAt = DateTime.UtcNow.AddDays(-2), BlockchainApi = "ETH", Json = "{}" },
                new() { CreatedAt = DateTime.UtcNow.AddDays(-3), BlockchainApi = "BTC", Json = "{}" }
            };

            await _context.BlockchainData.AddRangeAsync(mockData);
            await _context.SaveChangesAsync();
            
            var (data, totalCount) = await _repository.GetAllAsync(skip: 0, pageSize: 2);

            
            Assert.Equal(3, totalCount);
            Assert.Equal(2, data.Count());
        }

        [Fact]
        public async Task GetAllByBlockchainApisAsync_ShouldReturnFilteredAndPaginatedData()
        {
            var mockData = new List<BlockchainData>
            {
                new() { CreatedAt = DateTime.UtcNow.AddDays(-1), BlockchainApi = "BTC", Json = "{}" },
                new() { CreatedAt = DateTime.UtcNow.AddDays(-2), BlockchainApi = "ETH", Json = "{}" },
                new() { CreatedAt = DateTime.UtcNow.AddDays(-3), BlockchainApi = "BTC", Json = "{}" }
            };

            await _context.BlockchainData.AddRangeAsync(mockData);
            await _context.SaveChangesAsync();

           var (data, totalCount) = await _repository.GetAllByBlockchainApisAsync("BTC", skip: 0, pageSize: 2);

            Assert.Equal(2, totalCount);
            Assert.Equal(2, data.Count());
            Assert.All(data, item => Assert.Equal("BTC", item.BlockchainApi));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}