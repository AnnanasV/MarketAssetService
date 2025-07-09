using MarketAssetService.Domain;
using Microsoft.EntityFrameworkCore;
using MarketAssetService.Application.Interfaces;

namespace MarketAssetService.Infrastructure.Repositories;

public class AssetRepository : IMarketAssetRepository
{
    private readonly AppDbContext _context;

    public AssetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MarketAsset>> GetAllAsync()
        => await _context.MarketAssets.ToListAsync();

    public async Task<MarketAsset?> GetBySymbolAsync(string symbol)
        => await _context.MarketAssets
            .Include(a => a.Prices)
            .FirstOrDefaultAsync(a => a.Symbol.ToLower() == symbol.ToLower());

    public async Task<MarketAsset?> GetByIdAsync(Guid id)
        => await _context.MarketAssets
            .Include(a => a.Prices)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task AddOrUpdateAsync(MarketAsset asset)
    {
        var existing = await GetBySymbolAsync(asset.Symbol);
        if (existing is null)
        {
            _context.MarketAssets.Add(asset);
        }
        else
        {
            existing.Description = asset.Description;
        }

        await _context.SaveChangesAsync();
    }
}

