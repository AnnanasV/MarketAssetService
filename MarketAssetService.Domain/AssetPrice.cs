namespace MarketAssetService.Domain;

public class AssetPrice
{
    public int Id { get; set; }
    public int MarketAssetId { get; set; }
    public MarketAsset? MarketAsset { get; set; }
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
}