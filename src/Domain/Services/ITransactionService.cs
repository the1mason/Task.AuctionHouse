using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services;
public interface ITransactionService
{
    public Task<AccountTransaction[]> GetAccountTransactionsAsync(long accountId, int limit, int offset);

    public Task<AccountTransaction> Deposit(long accountId, long amount);

    public Task<AccountTransaction> Withdraw(long accountId, long amount);
}
