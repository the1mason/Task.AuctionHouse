namespace Domain.Models;

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


    public bool CanAfford(long amount)
    {
        return Balance >= amount;
    }

    public void ReserveFunds(long amount)
    {
        if (!CanAfford(amount))
            throw new InvalidOperationException("Not enough money to reserve");

        Balance -= amount;
        ReservedAmount += amount;
    }

    public void ReleaseFunds(long amount)
    {
        if (ReservedAmount < amount)
            throw new InvalidOperationException("Not enough money to release");

        Balance += amount;
        ReservedAmount -= amount;
    }

    public void WithdrawFunds(long amount)
    {
        if (!CanAfford(amount))
            throw new InvalidOperationException("Not enough money to withdraw");

        Balance -= amount;
    }

    public void DepositFunds(long amount)
    {
        Balance += amount;
    }

}
