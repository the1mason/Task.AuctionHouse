using Domain.Models;
using Domain.Services.Results;

namespace Domain.Services;
public interface IAccountService
{
    Task<Account?> GetAccountAsync(long accountId);

    Task<Account[]> GetAccountsAsync(int skip, int take, string? login = null, Role? role = null,
        bool includeDeleted = false, bool includeBlocked = false);

    Task<AccountResult> CreateAccountAsync(string login, string password, Role role);

    Task<AccountResult> ChangePasswordAsync(long accountId, string oldPassword, string newPassword);

    Task<AccountResult> ChangeRoleAsync(long accountId, Role role);

    Task<AccountResult> SetAccountBlockStatusAsync(long accountId, bool status);

    Task<AccountResult> DeleteAccountAsync(long accountId);

    Task<AuthenticateAccountResult> AuthenticateAsync(string login, string password);
}
