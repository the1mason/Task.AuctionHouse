using Domain.Models;
using Domain.Services.Results;
namespace Domain.Services;
public interface IRefreshTokenService
{
    public Task<RefreshToken> WriteRefreshTokenAsync(long userId, string token);

    public Task<RefreshTokenResult> GetRefreshTokenAsync(string token);
}
