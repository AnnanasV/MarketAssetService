using MarketAssetService.Domain;
using Microsoft.EntityFrameworkCore;

namespace MarketAssetService.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MarketAsset> MarketAssets { get; set; }
    public DbSet<AssetPrice> AssetPrices { get; set; }
}
