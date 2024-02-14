using Domain.Contracts.Models;
using Domain.Models;
namespace Domain.Services;

public interface IAccessTokenService
{
    Token GenerateAccessToken(Account account);
}