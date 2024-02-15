using Domain.Models;
using Domain.Services;
using Domain.Services.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class LotsController : ControllerBase
{
    private readonly TimeProvider _timeProvider;
    private readonly ILotService _lotService;
    public LotsController(TimeProvider timeProvider, ILotService lotService)
    {
        _timeProvider = timeProvider;
        _lotService = lotService;
    }

    [HttpGet]
    public async Task<IResult> GetLots(int limit = 10, int offset = 0, string? title = null, long? minPrice = null, long? maxPrice = null,
               DateTimeOffset? opensAt = null, DateTimeOffset? closesAt = null, long? accountId = null, bool includeClosed = false, bool includeDeleted = false)
    {
        if (limit < 1 || limit > 100)
            return Results.BadRequest();

        if (offset < 0)
            return Results.BadRequest();

        if (minPrice.HasValue && minPrice < 0)
            return Results.BadRequest();

        if (maxPrice.HasValue && maxPrice < 0)
            return Results.BadRequest();

        var lots = await _lotService.GetLotsAsync(limit, offset, title, minPrice, maxPrice, opensAt, closesAt, accountId, includeClosed, includeDeleted);
        return Results.Ok(lots.Select(l =>
        new GetLotResult(l.Id, l.Title, l.Description, l.MinPrice, l.OpeningAt, l.ClosingAt, l.SellerId, l.Seller!.Login, l.WinnerId, l.Winner?.Login, l.IsDeleted)));
    }

    [HttpGet("{id}")]
    public async Task<IResult> GetLot(long id)
    {
        var lot = await _lotService.GetLotAsync(id);
        if (lot == null)
            return Results.NotFound();

        return Results.Ok(new GetLotResult(lot.Id, lot.Title, lot.Description, lot.MinPrice, lot.OpeningAt, lot.ClosingAt, 
            lot.SellerId, lot.Seller!.Login, lot.WinnerId, lot.Winner?.Login, lot.IsDeleted));
    }

    [Authorize]
    [HttpPost]
    public async Task<IResult> CreateLot(LotRequest request)
    {
        var hasId = long.TryParse(User.FindFirstValue("AccountId"), out var accountId);

        if (!hasId)
            return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            return Results.BadRequest("Invalid title or description");

        if (request.MinPrice < 0)
            return Results.BadRequest();

        if (request.OpeningAt < _timeProvider.GetUtcNow() || request.ClosingAt < request.OpeningAt)
            return Results.BadRequest("Invalid opening/closing dates");



        var result = await _lotService.CreateLotAsync(request.Title, request.Description, request.MinPrice, request.OpeningAt, request.ClosingAt, accountId);

        if (result.IsLotCreateError)
            return result.AsLotCreateError switch
            {
                LotCreateError.InvalidCreatedAt => Results.BadRequest("Invalid created at date"),
                LotCreateError.InvalidClosedAt => Results.BadRequest("Invalid closed at date"),
                LotCreateError.InvalidPrice => Results.BadRequest("Invalid price"),
                _ => Results.StatusCode(500)
            };
        var lot = result.AsLot;
        return Results.Ok(new GetLotResult(lot.Id, lot.Title, lot.Description, lot.MinPrice, lot.OpeningAt, lot.ClosingAt, accountId, null, null, null, lot.IsDeleted));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IResult> UpdateLot([FromRoute] long id,[FromBody]LotRequest request)
    {
        var hasId = long.TryParse(User.FindFirstValue("AccountId"), out var accountId);
        var hasRole = int.TryParse(User.FindFirstValue("Role"), out var accountRole);

        if (!hasId || !hasRole)
            return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            return Results.BadRequest("Invalid title or description");

        if (request.MinPrice < 0)
            return Results.BadRequest();

        if (request.OpeningAt < _timeProvider.GetUtcNow() || request.ClosingAt < request.OpeningAt)
            return Results.BadRequest("Invalid opening/closing dates");

        var result = await _lotService.UpdateLotAsync(id, request.Title, request.Description, request.MinPrice, request.OpeningAt, request.ClosingAt, accountId, (Role)accountRole);
    
        if(result.IsLotUpdateError)
            return result.AsLotUpdateError switch
            {
                LotUpdateError.NotFound => Results.NotFound(),
                LotUpdateError.InvalidCreatedAt => Results.BadRequest("Invalid created at date"),
                LotUpdateError.InvalidClosedAt => Results.BadRequest("Invalid closed at date"),
                LotUpdateError.InvalidPrice => Results.BadRequest("Invalid price"),
                LotUpdateError.AlreadyClosed => Results.BadRequest("Lot is already closed"),
                LotUpdateError.AlreadyOpen => Results.BadRequest("Lot is already open"),
                _ => Results.StatusCode(500)
            };

        var lot = result.AsLot;

        return Results.Ok(new GetLotResult(lot.Id, lot.Title, lot.Description, lot.MinPrice, lot.OpeningAt, lot.ClosingAt, lot.SellerId, null, null, null, lot.IsDeleted));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IResult> DeleteLot(long id, [FromQuery] bool force = false)
    {
        var hasId = long.TryParse(User.FindFirstValue("AccountId"), out var accountId);
        var hasRole = int.TryParse(User.FindFirstValue("Role"), out var accountRole);

        if (!hasId || !hasRole)
            return Results.Unauthorized();

        var result = await _lotService.DeleteLotAsync(id, accountId, (Role)accountRole, force);

        if (result.IsLotUpdateError)
            return result.AsLotUpdateError switch
            {
                LotUpdateError.NotFound => Results.NotFound(),
                LotUpdateError.AlreadyClosed => Results.BadRequest("Lot is already closed"),
                LotUpdateError.AlreadyOpen => Results.BadRequest("Lot is already open"),
                _ => Results.StatusCode(500)
            };

        return Results.Ok();
    }

    public record GetLotResult(long Id, string Title, string Description, long MinPrice, DateTimeOffset OpensAt, DateTimeOffset ClosesAt,
        long SellerId, string? SellerName, long? WinnerId, string? WinnerName, bool isDeleted);

    public record LotRequest(string Title, string Description, long MinPrice, DateTimeOffset OpeningAt, DateTimeOffset ClosingAt);

}