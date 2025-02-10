using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BlockchainApp.Infrastructure.Context
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            string srcPath = Directory.GetCurrentDirectory();
            string solutionPath = Directory.GetParent(srcPath)?.FullName;
            if (solutionPath == null)
            {
                throw new InvalidOperationException("Unknown path");
            }

            
            string appPath = Path.Combine(solutionPath, "BlockchainApp");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(appPath) 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            optionsBuilder.UseNpgsql(connectionString); 

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}