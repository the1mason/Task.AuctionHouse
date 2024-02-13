namespace Domain.Models;

public class Account
{
    public long Id { get; set; }

    public required string Login { get; set; }

    public string? PasswordHash { get; set; } = null;

    public int RoleId { get; set; }

    public long Balance { get; set; }

    public long ReservedAmount { get; set; }

    public Role? Role { get; set; } = null;

    public bool IsBlocked { get; set; }

    public bool IsDeleted { get; set; }
}
