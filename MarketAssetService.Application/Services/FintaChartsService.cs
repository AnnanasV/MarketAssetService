using MarketAssetService.Application.DTOs;
using MarketAssetService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MarketAssetService.Infrastructure.Services;

public class FintaChartsService : IFintaChartsService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly ILogger<FintaChartsService> _logger;
    private readonly IConfiguration _config;

    public FintaChartsService(HttpClient httpClient, IAuthService authService, ILogger<FintaChartsService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _authService = authService;
        _logger = logger;
        _config = config;
    }


    public async Task<List<FintaInstrumentDto>> GetInstrumentsAsync(string provider = "oanda", string kind = "forex")
    {
        var token = await _authService.GetAccessTokenAsync();

        var uri = $"{_config["Finta:BaseUri"]}/api/instruments/v1/instruments?provider={provider}&kind={kind}";

        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(content).RootElement;

        _logger.LogInformation("Raw JSON: {Json}", content);

        var instruments = new List<FintaInstrumentDto>();
        if (!root.TryGetProperty("data", out var dataElement) || dataElement.ValueKind != JsonValueKind.Array)
        {
            _logger.LogError("Expected 'data' array not found in response: {Json}", content);
            throw new Exception("Invalid API response: missing 'data' array");
        }

        foreach (var item in dataElement.EnumerateArray())
        {
            instruments.Add(new FintaInstrumentDto
            {
                Id = item.GetProperty("id").GetString()!,
                Symbol = item.GetProperty("symbol").GetString()!,
                Description = item.GetProperty("description").GetString()!
            });
        }

        return instruments;
    }


    public async Task<HistoricalPriceDto?> GetLatestPriceAsync(Guid instrumentId, string provider = "oanda")
    {
        var token = await _authService.GetAccessTokenAsync();

        var uri = $"{_config["Finta:BaseUri"]}/api/bars/v1/bars/count-back" +
                  $"?instrumentId={instrumentId}&provider={provider}&interval=1&periodicity=minute&barsCount=1";

        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Price not found for {InstrumentId}", instrumentId);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Raw JSON response: {Json}", content);
        var root = JsonDocument.Parse(content).RootElement;

        if (!root.TryGetProperty("data", out var dataElement) || dataElement.ValueKind != JsonValueKind.Array)
        {
            _logger.LogError("Expected 'data' array not found in response: {Json}", content);
            return null;
        }

        var priceItem = dataElement.EnumerateArray().FirstOrDefault();
        if (priceItem.ValueKind == JsonValueKind.Undefined)
            return null;

        return new HistoricalPriceDto
        {
            Price = priceItem.GetProperty("c").GetDecimal(), // "c" - close
            Timestamp = priceItem.GetProperty("t").GetDateTime() // "t" - timestamp
        };
    }
}
