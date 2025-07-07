using System.Text.Json.Serialization;

namespace MarketAssetService.Application.DTOs;

public class AuthTokenResponse
{
    [JsonPropertyName("access_token")]
    public string Access_Token { get; set; }

    [JsonPropertyName("expires_in")]
    public int Expires_In { get; set; }
}