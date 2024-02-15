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
        if (account is null)
            return AuthenticateAccountError.NotFound;

        if (account.IsBlocked)
            return AuthenticateAccountError.Blocked;

        var isPasswordValid = _passwordHasher.VerifyPassword(password, account.PasswordHash!);

        if (!isPasswordValid)
            return AuthenticateAccountError.Unauthorized;

        return account;
    }

    public async Task<AccountResult> CreateAccountAsync(string login, string password, Role role)
    {
        var passwordHash = _passwordHasher.HashPassword(password);

        var account = new Account
        {
            Login = login,
            PasswordHash = passwordHash,
            Role = role,
            IsBlocked = false,
            Balance = 0,
            ReservedAmount = 0,
            IsDeleted = false
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return account;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE constraint failed"))
            {
                return AccountError.AlreadyExists;
            }
            throw;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Account?> GetAccountAsync(long accountId)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        return account;
    }

    public async Task<Account[]> GetAccountsAsync(int skip, int take, string? login = null, Role? role = null, 
        bool includeDeleted = false, bool includeBlocked = false)
    {
        var query = _dbContext.Accounts.AsQueryable();

        if (login is not null)
            query = query.Where(a => a.Login.Contains(login));

        if (role is not null)
            query = query.Where(a => a.Role == role);

        if (!includeDeleted)
            query = query.Where(a => !a.IsDeleted);

        if (!includeBlocked)
            query = query.Where(a => !a.IsBlocked);

        return await query.Skip(skip).Take(take).ToArrayAsync();
    }

    public async Task<AccountResult> ChangePasswordAsync(long accountId, string oldPassword, string newPassword)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        if (account is null)
            return AccountError.NotFound;

        var isPasswordValid = _passwordHasher.VerifyPassword(oldPassword, account.PasswordHash!);

        if (!isPasswordValid)
            return AccountError.Unauthorized;

        var newPasswordHash = _passwordHasher.HashPassword(newPassword);
        account.PasswordHash = newPasswordHash;

        await _dbContext.SaveChangesAsync();

        return account;
    }
      
    public async Task<AccountResult> ChangeRoleAsync(long accountId, Role role)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        if (account is null)
            return AccountError.NotFound;

        account.Role = role;

        await _dbContext.SaveChangesAsync();

        return account;
    }


    public async Task<AccountResult> DeleteAccountAsync(long accountId)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        if (account is null)
            return AccountError.NotFound;

        account.IsDeleted = true;

        await _dbContext.SaveChangesAsync();

        return account;
    }

    public async Task<AccountResult> SetAccountBlockStatusAsync(long accountId, bool status)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        if (account is null)
            return AccountError.NotFound;

        account.IsBlocked = status;

        await _dbContext.SaveChangesAsync();

        return account;
    }
}
