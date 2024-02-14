using Domain.Models;
using Domain.Services.Results;

namespace Domain.Services;
public interface IAccountService
{
    Task<Account> GetAccountAsync(long accountId);

    Task<Account> CreateAccountAsync(string login, string password, Role role);

    Task<Account> ChangePasswordAsync(long accountId, string oldPassword, string newPassword);

    Task<Account> AuthenticateUserAsync(string login, string password);

    Task<Account> ChangeRoleAsync(long accountId, Role role);

    Task<Account> SetAccountBlockStatusAsync(long accountId, bool status);

    Task<Account> DeleteAccountAsync(long accountId);

    Task<AuthenticateAccountResult> AuthenticateAsync(string login, string password);
}
