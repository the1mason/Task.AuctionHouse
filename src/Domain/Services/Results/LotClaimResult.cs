using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;


[UnionType(typeof(Lot))]
[UnionType(typeof(LotClaimError))]
public readonly partial struct LotClaimResult
{

}

public enum LotClaimError
{
    NotFound,
    Unauthorized,
    NotWinner,
    NotClosed,
    NotOpen,
    Claimed
}