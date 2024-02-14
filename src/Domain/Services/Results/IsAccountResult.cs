using Domain.Models;
using RhoMicro.Unions;

namespace Domain.Services.Results;

[UnionType(typeof(InvalidCredentialsError))]
[UnionType(typeof(Account))]
public readonly partial struct AuthenticateAccountResult { }

public readonly struct InvalidCredentialsError
{
    public string Login { get; }

    public InvalidCredentialsError(string login)
    {
        Login = login;
    }
}
