namespace MarketAssetService.Domain;

public class AssetPrice
{
    public int Id { get; set; }

    public Guid AssetId { get; set; }
    public MarketAsset Asset { get; set; } = null!;

    public decimal Price { get; set; }

    public DateTime Timestamp { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}