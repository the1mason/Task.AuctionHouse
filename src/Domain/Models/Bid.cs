namespace Domain.Models;

public class Bid
{
    public long Id { get; set; }

    public long LotId { get; set; }

    public long AccountId { get; set; }

    public long TransactionId { get; set; }

    public required long Price { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Lot? Lot { get; set; } = null;

    public Account? Account { get; set; } = null;

    public AccountTransaction? AccountTransaction { get; set; } = null;
}
