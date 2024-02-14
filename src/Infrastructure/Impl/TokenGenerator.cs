using Domain.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Domain.Contracts;
using Domain.Contracts.Models;

namespace Infrastructure.Impl;
public class TokenGenerator : ITokenGenerator
{

    private readonly TokenOptions _tokenOptions;
    private readonly TimeProvider _timeProvider;

    public TokenGenerator(IOptions<TokenOptions> tokenOptions, TimeProvider timeProvider)
    {
        _tokenOptions = tokenOptions.Value;
        _timeProvider = timeProvider;
    }

    public Token GenerateAccessToken(Account account)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.ASCII.GetBytes(_tokenOptions.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Login),
                new Claim("Role", account.Role.ToString()),
                new Claim("AccountId", account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             }),
            Expires = _timeProvider.GetUtcNow().DateTime + _tokenOptions.AccessTokenLifetime,
            Issuer = _tokenOptions.Issuer,
            Audience = _tokenOptions.Audience,
            IssuedAt = _timeProvider.GetUtcNow().DateTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha512Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        return new Token(tokenString, tokenDescriptor.Expires.Value);
    }

    public Token GenerateRefreshToken()
    {
        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(72));
        return new Token(token, _timeProvider.GetUtcNow().DateTime + _tokenOptions.RefreshTokenLifetime);
    }
}
