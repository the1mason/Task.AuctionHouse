using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;

[UnionType(typeof(AccountTransaction))]
[UnionType(typeof(CreateAccountTransactionError))]
public readonly partial struct CreateAccountTransactionResult
{
}

public enum CreateAccountTransactionError
{
    AccountNotFound,
    InsufficientFunds,
    InvalidTransactionType
}