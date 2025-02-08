using BlockchainApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlockchainApp.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<BlockchainData> BlockchainData { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlockchainData>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Json)
                    .IsRequired();

                entity.Property(e => e.BlockchainApi)
                    .IsRequired();

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.BlockchainApi);
                entity.HasIndex(e => new { e.BlockchainApi, e.CreatedAt });
            });
        }
    }
}