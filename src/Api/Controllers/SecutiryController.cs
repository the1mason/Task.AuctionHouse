using Domain.Models;
using Domain.Services;
using Domain.Services.Results;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SecutiryController : ControllerBase
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAccountService _accountService;
    private readonly ISecurityService _securityService;
    private readonly IConfiguration _configuration;
    private readonly TimeProvider _timeProvider;

    public SecutiryController(IRefreshTokenService refreshTokenService,
        IAccountService accountService,
        ISecurityService securityService,
        IConfiguration configuration,
        TimeProvider timeProvider)
    {
        _refreshTokenService = refreshTokenService;
        _accountService = accountService;
        _securityService = securityService;
        _configuration = configuration;
        _timeProvider = timeProvider;        
    }

    [HttpPost]
    [Route("token")]
    public async Task<IResult> CreateToken([FromBody] CreateTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest();

        var accountResult = await _accountService.AuthenticateAsync(request.Login, request.Password);

        if (accountResult.IsInvalidCredentialsError)
            return Results.Unauthorized();

        Account account = accountResult.AsAccount;

        string newRefreshToken = await Task.Run(_securityService.GenerateTokenAsync);
        var refreshToken = await _refreshTokenService.WriteRefreshTokenAsync(account.Id, newRefreshToken);

        // i'll move it (hopefully!)
        (string jwtToken, DateTime expires) = GenerateJWT(account, _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], _configuration["Jwt:Key"])

        return Results.Ok(new CreateTokenResponse(jwtToken, expires, refreshToken.Token, refreshToken.AccountId, refreshToken.ExpiredAt));
    }
    public record CreateTokenRequest(string Login, string Password);
    public record CreateTokenResponse(string Token, DateTime Expires, string RefreshToken, long AccountId, DateTimeOffset RefreshTokenExpires);

    [HttpPost]
    [Route("token/refresh")]
    public async Task<IResult> RefreshToken([FromBody] string refreshToken)
    {
        if(string.IsNullOrWhiteSpace(refreshToken))
            return Results.BadRequest();

        var refreshTokenResult = await _refreshTokenService.GetRefreshTokenAsync(refreshToken);

        if (refreshTokenResult.IsRefreshTokenError)
            return refreshTokenResult.AsRefreshTokenError switch
            {
                RefreshTokenError.NotFound => Results.NotFound(),
                RefreshTokenError.Revoked => Results.Forbid(),
                RefreshTokenError.Expired => Results.Unauthorized(),
                _ => Results.BadRequest()
            };

        var storedRefreshToken = refreshTokenResult.AsRefreshToken;

        // i'll move it (hopefully!)
        (string jwtToken, DateTime expires) = GenerateJWT(storedRefreshToken.Account!, _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], _configuration["Jwt:Key"]);

        return Results.Ok(new RefreshTokenResponse(jwtToken, expires));
    }

    public record RefreshTokenResponse(string jwt, DateTime expires);

    private (string token, DateTime expires) GenerateJWT(Account account, string issuer, string audience, string key)
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
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
             }),
            Expires = _timeProvider.GetUtcNow().DateTime.AddMinutes(5),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha512Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        return (tokenString, tokenDescriptor.Expires.Value);
    }
}
