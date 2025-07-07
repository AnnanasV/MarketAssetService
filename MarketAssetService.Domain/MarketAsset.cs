namespace MarketAssetService.Domain;

public class MarketAsset
{
    public string Id { get; set; }
    public string Symbol { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}