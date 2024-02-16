namespace Domain.Models;

public class AccountTransaction
{
    public long Id { get; set; }

    public long SenderId { get; set; }
    public long RecipientId { get; set; }

    public long Amount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Account? Sender { get; set; } = null;
    public Account? Recipient { get; set; } = null;
    public TransactionType Type { get; set; }
}
