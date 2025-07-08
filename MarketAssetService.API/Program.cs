using MarketAssetService.Application.Interfaces;
using MarketAssetService.Domain;
using MarketAssetService.Infrastructure;
using MarketAssetService.Infrastructure.Repositories;
using MarketAssetService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace MarketAssetService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls("http://0.0.0.0:5055");

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            builder.Services.AddMemoryCache();

            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IFintaChartsService, FintaChartsService>();

            builder.Services.AddScoped<IMarketAssetRepository, AssetRepository>();
            builder.Services.AddScoped<IAssetPriceRepository, AssetPriceRepository>();

            builder.Services.AddControllers();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapGet("/", () => "API is running");

            app.Run();
        }
    }
}