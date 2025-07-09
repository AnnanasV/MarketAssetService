using MarketAssetService.Domain;
using System.Text.Json.Serialization;

namespace MarketAssetService.Application.DTOs;

public class FintaInstrumentDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    public Dictionary<string, AssetMapping> Mappings { get; set; } = new();
}
