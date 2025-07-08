using MarketAssetService.Application.DTOs;
using MarketAssetService.Application.Interfaces;
using MarketAssetService.Domain;
using MarketAssetService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketAssetService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IFintaChartsService _finta;
    private readonly IMarketAssetRepository _assetRepository;
    private readonly IAssetPriceRepository _priceRepository;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(IFintaChartsService finta, IMarketAssetRepository assetRepository, IAssetPriceRepository priceRepository, ILogger<AssetsController> logger)
    {
        _finta = finta;
        _assetRepository = assetRepository;
        _priceRepository = priceRepository;
        _logger = logger;
    }

    // GET /api/assets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarketAsset>>> GetSupportedAssets()
    {
        var instruments = await _finta.GetInstrumentsAsync();

        foreach (var instrument in instruments)
        {
            if (!Guid.TryParse(instrument.Id, out var parsedId))
            {
                _logger.LogWarning("Invalid GUID from instrument: {Id}", instrument.Id);
                continue;
            }

            var existing = await _assetRepository.GetBySymbolAsync(instrument.Symbol);
            if (existing is null)
            {
                var newAsset = new MarketAsset
                {
                    Id = parsedId,
                    Symbol = instrument.Symbol,
                    Description = instrument.Description
                };

                await _assetRepository.AddOrUpdateAsync(newAsset);
            }
        }

        var allAssets = await _assetRepository.GetAllAsync();
        return Ok(allAssets);
    }

    // GET /api/assets/price?symbol=
    [HttpGet("price")]
    public async Task<IActionResult> GetPriceBySymbol([FromQuery] string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest("Symbol is required.");

        var instruments = await _finta.GetInstrumentsAsync();
        var asset = await _assetRepository.GetBySymbolAsync(symbol);
        if (asset is null)
            return NotFound($"Asset '{symbol}' is not supported. Try /api/assets to sync supported assets.");

        var priceDto = await _finta.GetLatestPriceAsync(asset.Id);
        if (priceDto is null)
            return NotFound($"No price data found for '{symbol}' from FintaCharts.");

        var existingPrice = await _priceRepository.GetLatestAsync(asset.Id);
        if (existingPrice is null || existingPrice.Timestamp != priceDto.Timestamp)
        {
            var price = new AssetPrice
            {
                AssetId = asset.Id,
                Symbol = asset.Symbol,
                Price = priceDto.Price,
                Timestamp = priceDto.Timestamp.ToUniversalTime()
            };

            await _priceRepository.AddAsync(price);
        }

        return Ok(new
        {
            Symbol = asset.Symbol,
            Name = asset.Description,
            Price = priceDto.Price,
            Timestamp = priceDto.Timestamp
        });
    }
}