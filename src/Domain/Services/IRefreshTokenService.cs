using Domain.Models;
using Domain.Services.Results;
namespace Domain.Services;
public interface IRefreshTokenService
{
    public Task<RefreshToken> CreateRefreshTokenAsync(Account account);

    public Task<RefreshTokenResult> GetRefreshToken(string token);
}
