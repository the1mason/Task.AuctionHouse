
using Domain.Contracts;
using Domain.Contracts.Models;
using Domain.Models;
using Domain.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services.Impl;
public class RefreshTokenService : IRefreshTokenService
{
    private readonly AuctionHouseContext _dbContext;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly TimeProvider _timeProvider;

    public RefreshTokenService(AuctionHouseContext dbContext, ITokenGenerator tokenGenerator, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
        _timeProvider = timeProvider;
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(Account account)
    {
        Token token = _tokenGenerator.GenerateRefreshToken();

        RefreshToken refreshToken = new()
        {
            Token = token.Value,
            ExpiredAt = token.Expires,
            CreatedAt = _timeProvider.GetUtcNow(),
            AccountId = account.Id,
            Account = account,
            IsRevoked = false
        };
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        
        try
        {
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return refreshToken;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    public async Task<RefreshTokenResult> GetRefreshToken(string token)
    {
        var refreshToken = await _dbContext.RefreshTokens.Include(r => r.Account).FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken is null)
            return RefreshTokenError.NotFound;

        if (refreshToken.IsRevoked)
            return RefreshTokenError.Revoked;

        if (refreshToken.ExpiredAt < _timeProvider.GetUtcNow())
            return RefreshTokenError.Expired;

        if (refreshToken.Account!.IsBlocked)
            return RefreshTokenError.Blocked;

        if (refreshToken.Account!.IsDeleted)
            return RefreshTokenError.Deleted;

        return refreshToken;
    }

    public async Task RevokeRefreshToken(string token)
    {
        var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken is null)
            return;

        refreshToken.Revoke();
        await _dbContext.SaveChangesAsync();
    }
}
