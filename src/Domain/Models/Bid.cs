namespace Domain.Models;

public class Bid
{
    public long Id { get; set; }

    public long LotId { get; set; }

    public long UserId { get; set; }

    public long TransactionId { get; set; }

    public required long Price { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Lot? Lot { get; set; } = null;

    public Account? User { get; set; } = null;

    public AccountTransaction AccountTransaction { get; set; } = null;
}
