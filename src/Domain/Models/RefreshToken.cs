using System.ComponentModel.DataAnnotations;

namespace Domain.Models;
public class RefreshToken
{
    [Key]
    public required string Token { get; set; }
    public long AccountId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiredAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public Account? Account { get; set; } = null;

    public bool IsValid(TimeProvider timeProvider)
    {
        if (IsRevoked) return false;

        if (timeProvider.GetUtcNow() > ExpiredAt) return false;

        return true;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}
