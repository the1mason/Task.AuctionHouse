namespace Domain.Models;

public class AccountTransaction
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public long Amount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Account? Account { get; set; } = null;

    public bool IsCancelled { get; set; }
}
