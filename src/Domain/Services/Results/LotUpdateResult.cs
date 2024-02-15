using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;


[UnionType(typeof(Lot))]
[UnionType(typeof(LotUpdateError))]
public readonly partial struct LotUpdateResult
{

}

public enum LotUpdateError
{
    NotFound,
    Unauthorized,
    AlreadyOpen,
    AlreadyClosed
}
