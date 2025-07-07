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

    public AssetsController(IFintaChartsService finta)
    {
        _finta = finta;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FintaInstrumentDto>>> GetSupportedAssets()
    {
        var instruments = await _finta.GetInstrumentsAsync();
        return Ok(instruments);
    }

    [HttpGet("price")]
    public async Task<ActionResult<object>> GetPriceBySymbol([FromQuery] string symbol)
    {
        var instruments = await _finta.GetInstrumentsAsync();
        var match = instruments.FirstOrDefault(i => i.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

        if (match is null)
            return NotFound($"Asset with symbol '{symbol}' not found");

        var price = await _finta.GetLatestPriceAsync(match.Id);
        if (price == null)
            return NotFound($"No price data found for '{symbol}'");

        return Ok(new
        {
            Symbol = match.Symbol,
            Name = match.Description,
            Price = price.Price,
            Timestamp = price.Timestamp
        });
    }
}