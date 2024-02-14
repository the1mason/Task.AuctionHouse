using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;

[UnionType(typeof(RefreshToken))]
[UnionType(typeof(RefreshTokenError))]
public readonly partial struct RefreshTokenResult
{
}

public enum RefreshTokenError
{
    NotFound,
    Expired,
    Revoked,
    Blocked
}