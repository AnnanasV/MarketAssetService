using MarketAssetService.Domain;

namespace MarketAssetService.Application.Interfaces;

public interface IMarketAssetRepository
{
    Task<IEnumerable<MarketAsset>> GetAllAsync();
    Task<MarketAsset?> GetBySymbolAsync(string symbol);
    Task<MarketAsset?> GetByIdAsync(Guid id);
    Task AddOrUpdateAsync(MarketAsset asset);
}
