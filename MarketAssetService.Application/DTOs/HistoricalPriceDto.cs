using System.Text.Json.Serialization;

namespace MarketAssetService.Application.DTOs;

public class HistoricalPriceDto
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    [JsonPropertyName("close")]
    public decimal Price { get; init; }
}