using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;

[UnionType(typeof(Account))]
[UnionType(typeof(AccountError))]
public readonly partial struct AccountResult
{
}

public enum AccountError
{
    NotFound,
    Unauthorized,
    AlreadyExists
}
