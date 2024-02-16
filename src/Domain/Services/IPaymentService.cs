using Domain.Models;
using Domain.Services.Results;

namespace Domain.Services;
public interface IPaymentService
{
    Task<AccountTransaction[]> GetAccountTransactionsAsync(long accountId, int limit, int offset, DateTimeOffset? startDate, DateTimeOffset? endDate);

    Task<CreateAccountTransactionResult> CreateAccountTransactionAsync(long accountId, long amount, TransactionType type);

    Task<AccountTransaction> AddTransactionAsync(AccountTransaction transaction);

}
