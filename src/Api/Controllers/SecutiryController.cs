using Domain.Models;
using Domain.Services;
using Domain.Services.Results;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SecutiryController : ControllerBase
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IAccountService _accountService;

    public SecutiryController(IRefreshTokenService refreshTokenService,
        IAccountService accountService,
        IAccessTokenService accessTokenService)
    {
        _refreshTokenService = refreshTokenService;
        _accountService = accountService;
        _accessTokenService = accessTokenService;
    }

    [HttpPost]
    [Route("token")]
    public async Task<IResult> CreateToken([FromBody] CreateTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest();

        var accountResult = await _accountService.AuthenticateAsync(request.Login, request.Password);

        if (accountResult.IsAuthenticateAccountError)
            return accountResult.AsAuthenticateAccountError switch
            {
                AuthenticateAccountError.NotFound => Results.NotFound(),
                AuthenticateAccountError.Blocked => Results.Forbid(),
                AuthenticateAccountError.Unauthorized => Results.Unauthorized(),
                _ => Results.StatusCode(500)
            };

        Account account = accountResult.AsAccount;

        var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(account);
        var accessToken = _accessTokenService.GenerateAccessToken(account);

        return Results.Ok(new CreateTokenResponse(accessToken.Value, accessToken.Expires, refreshToken.Token, refreshToken.AccountId, refreshToken.ExpiredAt));
    }

    public record CreateTokenRequest(string Login, string Password);
    public record CreateTokenResponse(string Token, DateTime Expires, string RefreshToken, long AccountId, DateTimeOffset RefreshTokenExpires);

    [HttpPost]
    [Route("token/refresh")]
    public async Task<IResult> RefreshToken([FromBody] string refreshToken)
    {
        if(string.IsNullOrWhiteSpace(refreshToken))
            return Results.BadRequest();

        var refreshTokenResult = await _refreshTokenService.GetRefreshToken(refreshToken);

        if (refreshTokenResult.IsRefreshTokenError)
            return refreshTokenResult.AsRefreshTokenError switch
            {
                RefreshTokenError.NotFound => Results.NotFound(), 
                RefreshTokenError.Revoked => Results.Forbid(),
                RefreshTokenError.Expired => Results.Unauthorized(),
                RefreshTokenError.Blocked => Results.Forbid(),
                RefreshTokenError.Deleted => Results.NotFound(),
                _ => Results.StatusCode(500)
            };

        var storedRefreshToken = refreshTokenResult.AsRefreshToken;

        var accessToken = _accessTokenService.GenerateAccessToken(storedRefreshToken.Account!);

        return Results.Ok(new RefreshTokenResponse(accessToken.Value, accessToken.Expires));
    }

    public record RefreshTokenResponse(string Jwt, DateTime Expires);

/*    private (string token, DateTime expires) GenerateJWT(Account account, string issuer, string audience, string key, TimeSpan lifetime)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.ASCII.GetBytes(key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Login),
                new Claim("Role", account.Role.ToString()),
                new Claim("AccountId", account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             }),
            Expires = _timeProvider.GetUtcNow().DateTime + lifetime,
            Issuer = issuer,
            Audience = audience,
            IssuedAt = _timeProvider.GetUtcNow().DateTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha512Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        return (tokenString, tokenDescriptor.Expires.Value);
    }*/
}
