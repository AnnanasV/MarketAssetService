namespace MarketAssetService.Application.Interfaces;

public interface IAssetSyncService
{
    Task SyncAsync(CancellationToken cancellationToken);
}
