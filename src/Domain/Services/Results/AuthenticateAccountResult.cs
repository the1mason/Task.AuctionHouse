using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;

[UnionType(typeof(AuthenticateAccountError))]
[UnionType(typeof(Account))]
public readonly partial struct AuthenticateAccountResult { }

public enum AuthenticateAccountError
{
    Unauthorized,
    Blocked,
    NotFound
}
