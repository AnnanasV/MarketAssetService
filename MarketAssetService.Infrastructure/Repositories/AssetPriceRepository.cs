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

    public async Task AddAsync(AssetPrice price)
    {
        _context.AssetPrices.Add(price);
        await _context.SaveChangesAsync();
    }
}