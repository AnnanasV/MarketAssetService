namespace MarketAssetService.Domain;

public class MarketAsset
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<AssetPrice> Prices { get; set; } = new();
}