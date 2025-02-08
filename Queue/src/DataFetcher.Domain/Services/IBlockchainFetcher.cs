namespace DataFetcher.Domain.Services;

public interface IBlockchainFetcher
{
    Task<string> FetchBlockchainDataAsync(string apiUrl);

}