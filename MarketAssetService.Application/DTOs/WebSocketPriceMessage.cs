using System.Text.Json.Serialization;

namespace MarketAssetService.Application.DTOs;

public class WebSocketPriceMessage
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("instrumentId")]
    public string? InstrumentId { get; set; }

    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    [JsonPropertyName("last")]
    public PriceDetail? Last { get; set; }

    [JsonPropertyName("bid")]
    public PriceDetail? Bid { get; set; }

    [JsonPropertyName("ask")]
    public PriceDetail? Ask { get; set; }

    [JsonPropertyName("quote")]
    public Quote? Quote { get; set; }
}
