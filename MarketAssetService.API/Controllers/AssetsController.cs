using MarketAssetService.Domain;
using MarketAssetService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketAssetService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssetsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarketAsset>>> GetAssets()
        {
            return await _context.MarketAssets.ToListAsync();
        }

        [HttpGet("{symbol}")]
        public async Task<ActionResult<object>> GetPrice(string symbol)
        {
            var asset = await _context.MarketAssets
                .Include(a => a.Prices)
                .FirstOrDefaultAsync(a => a.Symbol == symbol);

            if (asset == null || asset.Prices.Count == 0)
                return NotFound();

            var latest = asset.Prices.OrderByDescending(p => p.Timestamp).First();

            return Ok(new
            {
                Symbol = asset.Symbol,
                Price = latest.Price,
                Timestamp = latest.Timestamp
            });
        }
    }
}
