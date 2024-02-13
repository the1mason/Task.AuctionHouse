using System.ComponentModel.DataAnnotations;

namespace Domain.Models;
public class Session
{
    [Key]
    public string Token { get; set; }
    public string UserName { get; set; }
    public long UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiredAt { get; set; }
    public bool IsRevoked { get; set; }
}
