namespace MarketAssetService.Domain;

public class MarketAsset
{
    public Guid Id { get; set; }
    public string Symbol { get; set; }
    public string Description { get; set; }
    public ICollection<AssetPrice> Prices { get; set; } = new List<AssetPrice>();
}