using Domain.Models;

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
        _dbContext.AccountTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync();
        return transaction;
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
