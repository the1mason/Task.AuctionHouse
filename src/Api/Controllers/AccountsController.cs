using Domain.Models;
using Domain.Services;
using Domain.Services.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{

    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest();

        var accountResult = await _accountService.CreateAccountAsync(request.Login, request.Password, Role.User);

        if (accountResult.IsAccountError)
            return accountResult.AsAccountError switch
            {
                AccountError.AlreadyExists => Results.Conflict(),
                _ => Results.StatusCode(500)
            };

        var account = accountResult.AsAccount;

        return Results.Ok(new RegisterResponse(account.Id, account.Login, account.Role));
    }
   

    [HttpGet("{id}")]
    public async Task<IResult> GetAccount([FromRoute] long id)
    {
        var account = await _accountService.GetAccountAsync(id);

        if(account is null)
            return Results.NotFound();

        return Results.Ok(new GetAccountResponse(account.Id, account.Login, account.Role, account.Balance));
    }


    [HttpGet]
    public async Task<IResult> GetAccounts(int skip = 0, int take = 10, string? login = null, Role? role = null,
        bool includeDeleted = false, bool includeBlocked = false)
    {

        if (skip < 0 || take < 1)
            return Results.BadRequest();

        if (take > 100)
            return Results.BadRequest();

        if (includeDeleted || includeBlocked)
        {
            var hasRole = int.TryParse(User.FindFirstValue("Role"), out var userRole);

            if (!hasRole)
                return Results.Unauthorized();

            if ((Role)userRole < Role.Moderator)
                return Results.Forbid();
        }
        var accounts = await _accountService.GetAccountsAsync(skip, take, login, role, includeDeleted, includeBlocked);
        return Results.Ok(accounts.Select(a => new GetAccountsResponse(a.Id, a.Login, a.Role, a.IsBlocked, a.IsDeleted)));
    }


    [Authorize]
    [HttpGet("me")]
    public async Task<IResult> GetMyAccount()
    {
        var hasId = long.TryParse(User.FindFirstValue("AccountId"), out var id);

        if(!hasId)
            return Results.Unauthorized();

        var account = await _accountService.GetAccountAsync(id);

        if (account is null)
            return Results.NotFound();

        return Results.Ok(new GetFullAccountResponse(account.Id, account.Login, account.Role, account.Balance, account.ReservedAmount));
    }

    [Authorize]
    [HttpPost("{id}/role")]
    public async Task<IResult> ChangeRole(long id, [FromBody] ChangeRoleRequest request)
    {
        var hasRole = int.TryParse(User.FindFirstValue("Role"), out var role);

        if (!hasRole)
            return Results.Unauthorized();

        if ((Role)role < Role.Admin)
            return Results.Forbid();

        var accountResult = await _accountService.ChangeRoleAsync(id, request.Role);

        if (accountResult.IsAccountError)
            return accountResult.AsAccountError switch
            {
                AccountError.NotFound => Results.NotFound(),
                _ => Results.StatusCode(500)
            };

        var account = accountResult.AsAccount;

        return Results.Ok(new ChangeRoleResponse(account.Id, account.Role));
    }


    [Authorize]
    [HttpPost("{id}/block")]
    public async Task<IResult> BlockAccount(long id)
    {
        return await SetBlock(id, true);
    }

    [Authorize]
    [HttpPost("{id}/unblock")]
    public async Task<IResult> UnblockAccount(long id)
    {
        return await SetBlock(id, false);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IResult> Delete(long id)
    {
        var hasRole = int.TryParse(User.FindFirstValue("Role"), out var role);

        if (!hasRole)
            return Results.Unauthorized();

        if ((Role)role < Role.Moderator)
            return Results.Forbid();

        var accountResult = await _accountService.DeleteAccountAsync(id);

        if (accountResult.IsAccountError)
            return accountResult.AsAccountError switch
            {
                AccountError.NotFound => Results.NotFound(),
                _ => Results.StatusCode(500)
            };

        return Results.Ok();
    }

    private async Task<IResult> SetBlock(long id, bool status)
    {
        var hasRole = int.TryParse(User.FindFirstValue("Role"), out var role);

        if (!hasRole)
            return Results.Unauthorized();

        if ((Role)role < Role.Moderator)
            return Results.Forbid();

        var accountResult = await _accountService.SetAccountBlockStatusAsync(id, status);

        if (accountResult.IsAccountError)
            return accountResult.AsAccountError switch
            {
                AccountError.NotFound => Results.NotFound(),
                _ => Results.StatusCode(500)
            };

        var account = accountResult.AsAccount;

        return Results.Ok(new BlockAccountResponse(account.Id, account.IsBlocked));
    }

    #region Records
    public record RegisterRequest(string Login, string Password);
    public record RegisterResponse(long AccountId, string Login, Role Role);
    public record GetAccountResponse(long AccountId, string Login, Role Role, long Balance);
    public record GetAccountsResponse(long AccountId, string Login, Role Role, bool IsBlocked, bool IsDeleted);
    public record GetFullAccountResponse(long AccountId, string Login, Role Role, long Balance, long ReservedAmount);
    public record ChangeRoleRequest(Role Role);
    public record ChangeRoleResponse(long AccountId, Role Role);
    public record BlockAccountResponse(long AccountId, bool IsBlocked);
    #endregion


}
