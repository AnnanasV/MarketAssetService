namespace MarketAssetService.Application.Interfaces;

public interface IAuthService
{
    Task<string> GetAccessTokenAsync();
}