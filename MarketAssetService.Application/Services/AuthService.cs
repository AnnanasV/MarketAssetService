using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MarketAssetService.Application.DTOs;
using MarketAssetService.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace MarketAssetService.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    private const string TokenCacheKey = "FintaAuthToken";

    public AuthService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _cache = cache;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_cache.TryGetValue(TokenCacheKey, out string token))
            return token;

        var uri = _configuration["Finta:TokenUrl"];
        var username = _configuration["Finta:Username"];
        var password = _configuration["Finta:Password"];

        var form = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", "app-cli" },
            { "username", username! },
            { "password", password! }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new FormUrlEncodedContent(form)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthTokenResponse>(json);

        if (result is null || string.IsNullOrWhiteSpace(result.Access_Token))
            throw new Exception("Authorization failed: No token received");

        _cache.Set(TokenCacheKey, result.Access_Token, TimeSpan.FromSeconds(result.Expires_In - 60));

        return result.Access_Token;
    }
}