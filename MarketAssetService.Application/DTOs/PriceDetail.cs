using System.Text.Json.Serialization;

namespace MarketAssetService.Application.DTOs;

public class PriceDetail
{
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("volume")]
    public decimal? Volume { get; set; }

    [JsonPropertyName("change")]
    public decimal? Change { get; set; }

    [JsonPropertyName("changePct")]
    public decimal? ChangePct { get; set; }
}
