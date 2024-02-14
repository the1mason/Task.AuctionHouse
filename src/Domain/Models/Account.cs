﻿namespace Domain.Models;

public class Account
{
    public long Id { get; set; }

    public required string Login { get; set; }

    public string? PasswordHash { get; set; } = null;

    public long Balance { get; set; }

    public long ReservedAmount { get; set; }

    public Role Role { get; set; }

    public bool IsBlocked { get; set; }

    public bool IsDeleted { get; set; }

}
