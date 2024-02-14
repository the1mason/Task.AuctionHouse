namespace Infrastructure;

public class TokenOptions
{
    public required string Key { get; set; }

    public required string Issuer { get; set; }

    public required string Audience { get; set; }

    public TimeSpan AccessTokenLifetime { get; set; }

    public TimeSpan RefreshTokenLifetime { get; set; }

    public bool ValidateAccessTokenLifetime { get; set; }

    public bool ValidateIssuer { get; set; }

    public bool ValidateAudience { get; set; }
}
