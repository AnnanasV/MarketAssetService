using MarketAssetService.Application.DTOs;
using MarketAssetService.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MarketAssetService.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthService> _logger;

    private const string TokenCacheKey = "FintaAuthToken";

    public AuthService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        _logger.LogInformation("AuthService.GetAccessTokenAsync called");

        var uri = _configuration["Finta:TokenUrl"];
        var username = _configuration["Finta:Username"];
        var password = _configuration["Finta:Password"];

        _logger.LogInformation($"Read config: TokenUrl={uri}, Username={username}, Password is {(string.IsNullOrEmpty(password) ? "empty" : "set")}");

        if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new Exception("Config parameters are missing!");
        }

        if (_cache.TryGetValue(TokenCacheKey, out string token))
        {
            _logger.LogInformation("Returning cached token");
            return token;
        }

        var form = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", "app-cli" },
            { "username", username },
            { "password", password }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new FormUrlEncodedContent(form)
        };

        var response = await _httpClient.SendAsync(request);

        _logger.LogInformation($"Response status: {response.StatusCode}");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"Response content: {json}");

        var result = JsonSerializer.Deserialize<AuthTokenResponse>(json);

        if (result == null || string.IsNullOrWhiteSpace(result.Access_Token))
        {
            _logger.LogError("Authorization failed: No token received");
            throw new Exception("Authorization failed: No token received");
        }

        _cache.Set(TokenCacheKey, result.Access_Token, TimeSpan.FromSeconds(result.Expires_In - 60));

        _logger.LogInformation("Token cached and returned");

        return result.Access_Token;
    }
}
