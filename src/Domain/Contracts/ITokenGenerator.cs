using Domain.Contracts.Models;
using Domain.Models;

namespace Domain.Contracts;
public interface ITokenGenerator
{
    Token GenerateAccessToken(Account account);

    Token GenerateRefreshToken();
}
