using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;

[UnionType(typeof(Bid))]
[UnionType(typeof(BidUpdateError))]
public readonly partial struct BidUpdateResult
{
}

public enum BidUpdateError
{
    BidNotFound,
    LotNotFound,
    InvalidPrice,
    LotClosed,
    LotNotOpen,
    BidLocked,
    NotOwner,
    AccountNotFound
}
