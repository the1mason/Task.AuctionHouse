using Domain.Models;
using Domain.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Impl;
public class SimplePaymentCallbackService : IPaymentCallbackService
{
    private readonly PaymentCallbackOptions _callbackOptions;
    private readonly IPaymentService _paymentService;
    public SimplePaymentCallbackService(IOptions<PaymentCallbackOptions> callbackOptions, IPaymentService paymentService)
    {
        _callbackOptions = callbackOptions.Value;
        _paymentService = paymentService;
    }

    public Task Deposit(long accountId, long amount, string key)
    {
        if (key != _callbackOptions.Key)
            throw new ArgumentException("Invalid key");

        return _paymentService.CreateAccountTransactionAsync(accountId, amount, TransactionType.Deposit);
    }

    public Task Withdraw(long accountId, long amount, string key)
    {
        if (key != _callbackOptions.Key)
            throw new ArgumentException("Invalid key");

        return _paymentService.CreateAccountTransactionAsync(accountId, amount, TransactionType.Withdrawal);
    }
}
