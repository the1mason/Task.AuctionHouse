using Domain.Contracts;
using Domain.Models;
using Domain.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services.Impl;
public class AccountService : IAccountService
{

    private readonly AuctionHouseContext _dbContext;

    private readonly IPasswordHasher _passwordHasher;

    public AccountService(AuctionHouseContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthenticateAccountResult> AuthenticateAsync(string login, string password)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Login == login);
        if(account is null)
            return AuthenticateAccountError.NotFound;

        if (account.IsBlocked)
            return AuthenticateAccountError.Blocked;

        var isPasswordValid = _passwordHasher.VerifyPassword(password, account.PasswordHash!);

        if(!isPasswordValid)
            return AuthenticateAccountError.Unauthorized;

        return account;
    }

    public async Task<Account> ChangePasswordAsync(long accountId, string oldPassword, string newPassword)
    {
        throw new NotImplementedException();
    }

    public async Task<Account> ChangeRoleAsync(long accountId, Role role)
    {
        throw new NotImplementedException();
    }

    public async Task<Account> CreateAccountAsync(string login, string password, Role role)
    {
        throw new NotImplementedException();
    }

    public async Task<Account> DeleteAccountAsync(long accountId)
    {
        throw new NotImplementedException();
    }

    public async Task<Account> GetAccountAsync(long accountId)
    {
        throw new NotImplementedException();
    }

    public Task<Account> SetAccountBlockStatusAsync(long accountId, bool status)
    {
        throw new NotImplementedException();
    }
}
