using MarketAssetService.Domain;

namespace MarketAssetService.Application.Interfaces;

public interface IAssetPriceRepository
{
    Task<AssetPrice?> GetLatestAsync(Guid assetId);
    Task AddOrUpdateAsync(AssetPrice price);
}
