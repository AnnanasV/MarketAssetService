using MarketAssetService.Application.Interfaces;
using MarketAssetService.Domain;
using MarketAssetService.Infrastructure;
using MarketAssetService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace MarketAssetService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls("http://0.0.0.0:5050");

            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient<IAuthService, AuthService>();

            builder.Services.AddControllers();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                if (!db.MarketAssets.Any())
                {
                    db.MarketAssets.AddRange(
                        new MarketAsset { Symbol = "EURUSD", Name = "Euro vs Dollar" },
                        new MarketAsset { Symbol = "GOOG", Name = "Google Stock" }
                    );
                    db.SaveChanges();
                }
            }

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}