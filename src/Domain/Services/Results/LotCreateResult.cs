using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;

[UnionType(typeof(Lot))]
[UnionType(typeof(LotCreateError))]
public readonly partial struct LotCreateResult
{
}

public enum LotCreateError
{
    InvalidPrice,
    InvalidCreatedAt,
    InvalidClosedAt
}