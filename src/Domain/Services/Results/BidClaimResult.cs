using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;


[UnionType(typeof(Lot))]
[UnionType(typeof(BidClaimError))]
public readonly partial struct BidClaimResult
{

}

public enum BidClaimError
{
    NotFound,
    NotWinner,
    NotClosed,
    NotOpen,
    Claimed
}