namespace MarketAssetService.Application.DTOs;

public class Quote
{
    public PriceDetail? Ask { get; set; }
    public PriceDetail? Bid { get; set; }
    public PriceDetail? Last { get; set; }
}
