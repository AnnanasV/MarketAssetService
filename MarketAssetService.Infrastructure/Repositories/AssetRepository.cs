using MarketAssetService.Domain;
using Microsoft.EntityFrameworkCore;
using MarketAssetService.Application.Interfaces;

namespace MarketAssetService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing market assets.
/// </summary>
public class AssetRepository : IMarketAssetRepository
{
    private readonly AppDbContext _context;

    public AssetRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all market assets from the database.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<MarketAsset>> GetAllAsync()
        => await _context.MarketAssets.ToListAsync();

    /// <summary>
    /// Retrieves a market asset by its symbol, including its prices.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public async Task<MarketAsset?> GetBySymbolAsync(string symbol)
        => await _context.MarketAssets
            .Include(a => a.Prices)
            .FirstOrDefaultAsync(a => a.Symbol.ToLower() == symbol.ToLower());

    /// <summary>
    /// Retrieves a market asset by its ID, including its prices.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<MarketAsset?> GetByIdAsync(Guid id)
        => await _context.MarketAssets
            .Include(a => a.Prices)
            .FirstOrDefaultAsync(a => a.Id == id);

    /// <summary>
    /// Adds or updates a market asset in the database.
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
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

