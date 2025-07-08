namespace MarketAssetService.Domain;

public class AssetPrice
{
    public Guid Id { get; set; }

    public Guid AssetId { get; set; }
    public MarketAsset Asset { get; set; } = null!;

    public string Symbol { get; set; }
    public decimal Price { get; set; }

    public DateTime Timestamp { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}