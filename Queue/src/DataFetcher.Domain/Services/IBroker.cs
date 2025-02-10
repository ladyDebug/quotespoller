namespace DataFetcher.Domain.Services
{
    public interface IBroker
    {
        Task PublishAsync(string key, object data);
    }
}
