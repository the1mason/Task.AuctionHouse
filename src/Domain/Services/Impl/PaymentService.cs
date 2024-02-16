using Domain.Models;
using Domain.Services.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Domain.Services.Impl;
public class PaymentService : IPaymentService
{
    private readonly AuctionHouseContext _dbContext;
    private readonly IAccountService _accountService;
    public PaymentService(AuctionHouseContext dbContext, IAccountService accountService)
    {
        _dbContext = dbContext;
        _accountService = accountService;
    }

    public async Task<CreateAccountTransactionResult> CreateAccountTransactionAsync(long accountId, long amount, TransactionType type)
    {
        var account = await _accountService.GetAccountAsync(accountId);
        if(account is null)
            return CreateAccountTransactionError.AccountNotFound;

        if (type == TransactionType.Withdrawal)
            amount = - amount;

        if (TransactionType.Transfer == type)
            return CreateAccountTransactionError.InvalidTransactionType;

        if(type == TransactionType.Withdrawal && !account.CanAfford(amount))
            return CreateAccountTransactionError.InsufficientFunds;
        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var transaction = new AccountTransaction
            {
                Amount = amount,
                SenderId = accountId,
                RecipientId = accountId,
                Type = type
            };

            account.Balance += amount;

            _dbContext.AccountTransactions.Add(transaction);

            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();
            return transaction;
        }
        catch (Exception)
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<AccountTransaction> AddTransactionAsync(AccountTransaction transaction)
    {

        var sender = await _dbContext.Accounts.FirstAsync(x => x.Id == transaction.SenderId);
        var receiver = await _dbContext.Accounts.FirstAsync(x => x.Id == transaction.RecipientId);

        sender.Balance -= transaction.Amount;
        receiver.Balance += transaction.Amount;

        _dbContext.AccountTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        return transaction;

    }
    public Task<AccountTransaction[]> GetAccountTransactionsAsync(long accountId, int limit, int offset, DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        var query = _dbContext.AccountTransactions.Where(x => x.RecipientId == accountId).OrderByDescending(x => x.CreatedAt).AsQueryable();

        if (startDate is not null)
            query = query.Where(x => x.CreatedAt >= startDate);

        if (endDate is not null)
                query = query.Where(x => x.CreatedAt <= endDate);
          
        return query.Skip(offset).Take(limit).ToArrayAsync();
    }
}
