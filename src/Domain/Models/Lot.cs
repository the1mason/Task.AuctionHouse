using Domain.Services.Results;
using System;

namespace Domain.Models;

public class Lot
{
    public long Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public long MinPrice { get; set; }

    public long CurrentPrice { get; set; }

    public long SellerId { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// When the lot will be opened for bids
    /// </summary>
    public DateTimeOffset OpeningAt { get; set; }

    public DateTimeOffset ClosingAt { get; set; }

    public long? WinnerId { get; set; }

    public Account? Seller { get; set; } = null;

    public Account? Winner { get; set; } = null;

    public ICollection<Bid> Bids { get; set; } = [];

    public bool IsDeleted { get; set; }

    public bool IsClosed(TimeProvider timeProvider)
    {
        return timeProvider.GetUtcNow() > ClosingAt;
    }

    public bool IsOpen(TimeProvider timeProvider)
    {
        return timeProvider.GetUtcNow() > OpeningAt;
    }

    public bool IsOpenForBids(TimeProvider timeProvider)
    {
        return IsOpen(timeProvider) && !IsClosed(timeProvider);
    }

    public bool IsOwnedBy(long accountId)
    {
        return SellerId == accountId;
    }

    public Lot SetOpeningAt(DateTimeOffset openingAt, TimeProvider timeProvider)
    {
        if (openingAt < timeProvider.GetUtcNow() + TimeSpan.FromMinutes(10))
        {
            throw new ArgumentException("CreatedAt should be after 10 minutes or more");
        }
        OpeningAt = openingAt;
        return this;
    }

    public Lot SetClosingAt(DateTimeOffset closingAt)
    {
        if(OpeningAt == default)
            throw new ArgumentException("OpeningAt should be set before ClosingAt");

        if (closingAt - OpeningAt < TimeSpan.FromHours(1))
            throw new ArgumentException("Lot should be available for bids for at least 1 hour");

        ClosingAt = closingAt;

        return this;

    }

    public Lot SetMinPice(long minPrice)
    {
        if(minPrice <= 0)
            throw new ArgumentException("MinPrice should be greater than 0");

        return this;

    }



}
