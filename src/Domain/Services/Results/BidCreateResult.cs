using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;

[UnionType(typeof(Bid))]
[UnionType(typeof(BidCreateError))]
public readonly partial struct BidCreateResult
{
}

public enum BidCreateError
{
    LotNotFound,
    AccountNotFound,
    InvalidPrice,
    LotClosed,
    LotNotOpen,
    BidLocked,
    CantBidSelf,
    InsufficientFunds
}
