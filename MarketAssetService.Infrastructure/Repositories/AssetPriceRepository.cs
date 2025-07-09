using MarketAssetService.Application.Interfaces;
using MarketAssetService.Domain;
using Microsoft.EntityFrameworkCore;

namespace MarketAssetService.Infrastructure.Repositories;

public class AssetPriceRepository : IAssetPriceRepository
{
    private readonly AppDbContext _context;

    public AssetPriceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AssetPrice?> GetLatestAsync(Guid assetId)
        => await _context.AssetPrices
            .Where(p => p.AssetId == assetId)
            .OrderByDescending(p => p.Timestamp)
            .FirstOrDefaultAsync();

    public async Task AddOrUpdateAsync(AssetPrice price)
    {
        var latest = await GetLatestAsync(price.AssetId);

        if (latest == null)
        {
            await _context.AssetPrices.AddAsync(price);
        }
        else
        {
            latest.Price = price.Price;
            latest.Symbol = price.Symbol;
            latest.Timestamp = price.Timestamp.ToUniversalTime();
            // Update existing price with new values
            _context.AssetPrices.Update(latest);
        }

        await _context.SaveChangesAsync();
    }
}