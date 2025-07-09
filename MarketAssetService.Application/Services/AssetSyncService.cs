using MarketAssetService.Application.Interfaces;
using MarketAssetService.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketAssetService.Application.Services;

public class AssetSyncService : IAssetSyncService
{
    private readonly ILogger<AssetSyncService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AssetSyncService(ILogger<AssetSyncService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task SyncAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var assetRepo = scope.ServiceProvider.GetRequiredService<IMarketAssetRepository>();
        var fintaService = scope.ServiceProvider.GetRequiredService<IFintaChartsService>();

        var instruments = await fintaService.GetInstrumentsAsync("oanda", "forex");

        foreach (var instrument in instruments)
        {
            if (!Guid.TryParse(instrument.Id, out var id))
                continue;

            var existing = await assetRepo.GetBySymbolAsync(instrument.Symbol);
            if (existing == null)
            {
                var newAsset = new MarketAsset
                {
                    Id = id,
                    Symbol = instrument.Symbol,
                    Description = instrument.Description
                };
                await assetRepo.AddOrUpdateAsync(newAsset);
                _logger.LogInformation("Added new asset {Symbol}", instrument.Symbol);
            }
            else
            {
                existing.Description = instrument.Description;
                await assetRepo.AddOrUpdateAsync(existing);
                _logger.LogInformation("Updated asset {Symbol}", instrument.Symbol);
            }
        }
    }
}
