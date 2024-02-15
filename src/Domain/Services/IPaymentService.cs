using Domain.Models;

namespace Domain.Services;
public interface IPaymentService
{
    Task<AccountTransaction[]> GetAccountTransactionsAsync(long accountId, int limit, int offset, DateTimeOffset? startDate, DateTimeOffset? endDate,
        bool includeCancelled = false, bool inclidePending = false);

    Task<AccountTransaction> GetAccountTransactionAsync(long id);

    Task<AccountTransaction> CreateAccountTransactionAsync(long accountId, long amount, TransactionType type);

    Task<AccountTransaction> CancelAccountTransactionAsync(long id);

    Task<AccountTransaction> CompleteAccountTransactionAsync(long id);

    Task<AccountTransaction> AddTransactionAsync(AccountTransaction transaction);

}
