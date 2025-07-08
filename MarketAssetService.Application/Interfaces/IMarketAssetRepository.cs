using MarketAssetService.Domain;

namespace MarketAssetService.Application.Interfaces;

public interface IMarketAssetRepository
{
    Task<IEnumerable<MarketAsset>> GetAllAsync();
    Task<MarketAsset?> GetBySymbolAsync(string symbol);
    Task<bool> ExistsAsync(string symbol);
    Task AddOrUpdateAsync(MarketAsset asset);
}
