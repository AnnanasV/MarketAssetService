using MarketAssetService.Application.DTOs;
using MarketAssetService.Application.Interfaces;
using MarketAssetService.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MarketAssetService.Application.Services;

public class FintaWebSocketService : BackgroundService
{
    private readonly ILogger<FintaWebSocketService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;

    public FintaWebSocketService(
    ILogger<FintaWebSocketService> logger,
    IServiceScopeFactory scopeFactory,
    IConfiguration config)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var scope = _scopeFactory.CreateScope();

                var syncService = scope.ServiceProvider.GetRequiredService<IAssetSyncService>();
                _logger.LogInformation("Running asset sync before WebSocket connection...");
                await syncService.SyncAsync(stoppingToken);

                using var client = new ClientWebSocket();
                using var authScope = _scopeFactory.CreateScope();
                var authService = authScope.ServiceProvider.GetRequiredService<IAuthService>();
                var token = await authService.GetAccessTokenAsync();

                var wsUrl = _config["Finta:WebSocketUrl"]?.Replace("{token}", token);
                _logger.LogInformation("Connecting to WebSocket: {Url}", wsUrl);
                await client.ConnectAsync(new Uri(wsUrl), stoppingToken);

                await SubscribeToInstrumentsAsync(client, scope, stoppingToken);

                while (client.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    var json = await ReceiveMessageAsync(client, stoppingToken);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        await HandleMessageAsync(json);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("The service is stopped.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while working with WebSocket. Reconnecting in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }

    private async Task SubscribeToInstrumentsAsync(ClientWebSocket client, IServiceScope scope, CancellationToken stoppingToken)
    {
        var chartService = scope.ServiceProvider.GetRequiredService<IFintaChartsService>();
        var instruments = await chartService.GetInstrumentsAsync("oanda", "forex");
        _logger.LogInformation("Instruments count from API: {Count}", instruments.Count());

        // Form and send subscription messages
        var subscriptionMessages = instruments
            .Select((instrument, index) => new
            {
                type = "l1-subscription",
                id = index.ToString(),
                instrumentId = instrument.Id,
                provider = "simulation",
                subscribe = true,
                kinds = new[] { "ask", "bid", "last" }
            })
            .ToList();

        foreach (var msg in subscriptionMessages)
        {
            var messageJson = JsonSerializer.Serialize(msg);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            var sendBuffer = new ArraySegment<byte>(messageBytes);

            await client.SendAsync(sendBuffer, WebSocketMessageType.Text, true, stoppingToken);
            _logger.LogInformation("Sent subscribe message: {message}", messageJson);

            // Prevent flooding the WebSocket server with messages
            await Task.Delay(50, stoppingToken);
        }
    }

    /// <summary>
    /// Get and process price message from WebSocket.
    /// </summary>
    /// <returns>String message</returns>
    private async Task<string> ReceiveMessageAsync(ClientWebSocket socket, CancellationToken token)
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        using var stream = new MemoryStream();

        WebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(buffer, token);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", token);
                return string.Empty;
            }

            stream.Write(buffer.Array!, buffer.Offset, result.Count);
        }
        while (!result.EndOfMessage);

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Add to database.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(string json)
    {
        _logger.LogInformation("HandleMessageAsync called with json: {json}", json);

        WebSocketPriceMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<WebSocketPriceMessage>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize WebSocket message");
            return;
        }

        if (message?.Type is "session" or "response")
        {
            _logger.LogDebug("Skipping non-price message of type: {type}", message.Type);
            return;
        }

        if (message?.InstrumentId == null || !Guid.TryParse(message.InstrumentId, out var instrumentId))
        {
            _logger.LogWarning("Invalid or missing instrumentId: {instrumentId}", message?.InstrumentId);
            return;
        }

        var priceDetail = GetBestPriceDetail(message);

        if (priceDetail?.Price is null || priceDetail?.Timestamp is null || priceDetail?.Volume is null)
        {
            _logger.LogWarning("Incomplete price detail in message: {json}", json);
            return;
        }

        if (priceDetail.Price <= 0 || priceDetail.Volume <= 0)
        {
            _logger.LogWarning("Invalid price values: {priceDetail}", JsonSerializer.Serialize(priceDetail));
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var assetRepo = scope.ServiceProvider.GetRequiredService<IMarketAssetRepository>();
        var priceRepo = scope.ServiceProvider.GetRequiredService<IAssetPriceRepository>();

        var asset = await assetRepo.GetByIdAsync(instrumentId);
        if (asset == null)
        {
            _logger.LogWarning("Asset not found for instrumentId: {instrumentId}", instrumentId);
            return;
        }

        var price = new AssetPrice
        {
            AssetId = asset.Id,
            Symbol = asset.Symbol,
            Price = priceDetail.Price.Value,
            Timestamp = priceDetail.Timestamp.Value.ToUniversalTime(),
            UpdatedAt = DateTime.UtcNow
        };

        await priceRepo.AddOrUpdateAsync(price);
        _logger.LogInformation("Updated price for {symbol}: {price} at {time}", asset.Symbol, price.Price, price.Timestamp);
    }

    private PriceDetail? GetBestPriceDetail(WebSocketPriceMessage message)
    {
        if (message.Quote != null)
            return message.Quote.Last ?? message.Quote.Bid ?? message.Quote.Ask;

        return message.Last ?? message.Bid ?? message.Ask;
    }
}
