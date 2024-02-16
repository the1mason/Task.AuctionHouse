using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Domain.Services.Impl;
public class PaymentService : IPaymentService
{
    private readonly AuctionHouseContext _dbContext;
    public PaymentService(AuctionHouseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AccountTransaction> CancelAccountTransactionAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<AccountTransaction> CompleteAccountTransactionAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<AccountTransaction> CreateAccountTransactionAsync(long accountId, long amount, TransactionType type)
    {
        throw new NotImplementedException();
    }

    public async Task<AccountTransaction> AddTransactionAsync(AccountTransaction transaction)
    {
        // this is dirty and utterly unacceptable :(
        // i'll revisit it and fix it later
        var dbContextTransaction = _dbContext.Database.CurrentTransaction;
        var isExternalTransaction = dbContextTransaction != null;
        dbContextTransaction ??= await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var sender = await _dbContext.Accounts.FirstAsync(x => x.Id == transaction.SenderId);
            var receiver = await _dbContext.Accounts.FirstAsync(x => x.Id == transaction.RecipientId);

            sender.Balance -= transaction.Amount;
            receiver.Balance += transaction.Amount;

            _dbContext.AccountTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            if (!isExternalTransaction)
            {
                await dbContextTransaction.CommitAsync();
            }

            return transaction;
        }
        catch (Exception)
        {
            if (!isExternalTransaction)
            {
                await dbContextTransaction.RollbackAsync();
            }

            throw;
        }
        finally
        {
            if (!isExternalTransaction)
            {
                await dbContextTransaction.DisposeAsync();
            }
        }
    }

    public Task<AccountTransaction> GetAccountTransactionAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<AccountTransaction[]> GetAccountTransactionsAsync(long accountId, int limit, int offset, DateTimeOffset? startDate, DateTimeOffset? endDate, bool includeCancelled = false, bool inclidePending = false)
    {
        throw new NotImplementedException();
    }
}
