using Domain.Contracts;
using Domain.Contracts.Models;
using Domain.Models;

namespace Domain.Services.Impl;
public class AccessTokenService : IAccessTokenService
{
    private readonly ITokenGenerator _tokenGenerator;

    public AccessTokenService(ITokenGenerator tokenGenerator)
    {
        _tokenGenerator = tokenGenerator;
    }

    public Token GenerateAccessToken(Account account)
    {
        return _tokenGenerator.GenerateAccessToken(account);
    }
}
