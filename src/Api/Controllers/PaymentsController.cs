using Domain.Services;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentCallbackService _paymentCallbackService;
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentCallbackService paymentCallbackService, IPaymentService paymentService)
    {
        _paymentCallbackService = paymentCallbackService;
        _paymentService = paymentService;
    }

    [HttpPost("deposit")]
    public async Task<IResult> DepositCallback([FromBody] PaymentRequest request)
    {
        try
        {
            await _paymentCallbackService.Deposit(request.AccountId, request.Amount, request.Key);
        }
        catch (ArgumentException)
        {
            return Results.BadRequest();
        }

        return Results.Ok();
    }

    [HttpPost("withdraw")]
    public async Task<IResult> WithdrawCallback([FromBody] PaymentRequest request)
    {
        try
        {
            await _paymentCallbackService.Withdraw(request.AccountId, request.Amount, request.Key);
        }
        catch (ArgumentException)
        {
            return Results.BadRequest();
        }

        return Results.Ok();
    }

    [Authorize]
    [HttpGet("/account/{id}")]
    public async Task<IResult> GetTransactions(long id, int limit = 10, int offset = 0, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        if(limit == 0 || limit > 100)
            return Results.BadRequest();

        if(offset < 0)
            return Results.BadRequest();

        var hasRole = int.TryParse(User.FindFirstValue("Role"), out var role);
        var hasAccount = int.TryParse(User.FindFirstValue("AccountId"), out var account);


        if (!hasRole && !hasAccount)
            return Results.Unauthorized();

        if (account != id || role < 1)
            return Results.Forbid();


        var balance = await _paymentService.GetAccountTransactionsAsync(id, limit, offset, startDate, endDate);
        return Results.Ok(balance);
    }

    public record class PaymentRequest(long AccountId, long Amount, string Key);

}
