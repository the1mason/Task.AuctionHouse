using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;
public interface IPaymentCallbackService
{
    public Task Deposit(long accountId, long amount, string key);

    public Task Withdraw(long accountId, long amount, string key);
}
