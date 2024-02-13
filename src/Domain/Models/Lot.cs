namespace Domain.Models;

public class Lot
{
    public long Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public long MinPrice { get; set; }

    public long CurrentPrice { get; set; }

    public long? SellerId { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// When the lot will be opened for bids
    /// </summary>
    public required DateTimeOffset OpeningAt { get; set; }

    public required DateTimeOffset ClosingAt { get; set; }

    public long? WinnerId { get; set; }

    public Account? Seller { get; set; } = null;

    public Account? Winner { get; set; } = null;

    public ICollection<Bid> Bids { get; set; } = [];

    public bool IsDeleted { get; set; }

}
