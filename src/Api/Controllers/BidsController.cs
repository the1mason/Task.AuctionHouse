using Domain.Services;
using Domain.Services.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;
    
    public BidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    [HttpPost]
    public async Task<IResult> CreateBid([FromBody] CreateBidRequest request)
    {
        var hasId = long.TryParse(User.FindFirstValue("AccountId"), out var accountId);

        if (!hasId)
            return Results.Unauthorized();

        if (request.Amount < 0)
            return Results.BadRequest();

        var bidResult = await _bidService.PlaceAsync(request.LotId, request.Amount, accountId);
        
        if (bidResult.IsBidCreateError)
            return bidResult.AsBidCreateError switch
            {
                BidCreateError.AccountNotFound => Results.NotFound("Unknown account"),
                BidCreateError.LotNotFound => Results.NotFound("Unknown lot"),
                BidCreateError.BidLocked => Results.BadRequest("You have the top bid on this lot."),
                BidCreateError.CantBidSelf => Results.BadRequest("Can't bid on your own lots"),
                BidCreateError.InsufficientFunds => Results.BadRequest("Insufficient funds"),
                BidCreateError.LotClosed => Results.BadRequest("Lot is closed for bids"),
                BidCreateError.LotNotOpen => Results.BadRequest("Lot is not open for bids"),
                BidCreateError.InvalidPrice => Results.BadRequest("Provided invalid value price"),
                _ => Results.StatusCode(500)
            };

        var bid = bidResult.AsBid;
        return Results.Ok(new CreateBidResponse(bid.Id, bid.LotId, bid.Price, bid.AccountId, bid.CreatedAt));
    }

    [HttpGet("{id}")]
    public async Task<IResult> GetBid([FromRoute] long id)
    {
        var bid = await _bidService.GetBidAsync(id);

        if (bid is null)
            return Results.NotFound();

        return Results.Ok(new GetBidResponse(bid.Id, bid.LotId, bid.Price, bid.AccountId, bid.CreatedAt));
    }

    [HttpGet("find")]
    public async Task<IResult> FindBid([FromQuery] long accountId, [FromQuery] long lotId)
    {
        var bid = await _bidService.FindBidAsync(accountId, lotId);

        if (bid is null)
            return Results.NotFound();

        return Results.Ok(new GetBidResponse(bid.Id, bid.LotId, bid.Price, bid.AccountId, bid.CreatedAt));
    }

    [HttpGet("lot/{id}")]
    public async Task<IResult> GetBidsForLot(long id, int limit = 100, int offset = 0, bool includeRecalled = true)
    {

        if (limit < 0 || limit > 100)
            return Results.BadRequest();

        if (offset < 0)
            return Results.BadRequest();

        var bids = await _bidService.GetBidsWithAccountsByLotAsync(id, limit, offset, includeRecalled);

        return Results.Ok(bids.Select(b => new GetBidResponseWithLogin(b.Id, b.LotId, b.Price, b.AccountId, b.Account!.Login, b.CreatedAt)));
    }

    [HttpGet("account/{id}")]
    public async Task<IResult> GetBidsForAccount(long id, int limit = 100, int offset = 0, bool includeRecalled = true)
    {
        if (limit < 0 || limit > 100)
            return Results.BadRequest();

        if (offset < 0)
            return Results.BadRequest();

        var bids = await _bidService.GetBidsWithLotsByAccountAsync(id, limit, offset, includeRecalled);

        return Results.Ok(bids.Select(b => new GetBidResponseWithLot(b.Id, b.LotId, b.Price, b.AccountId, new (b.LotId, b.Lot!.Title, b.Lot!.MinPrice, b.Lot!.OpeningAt, b.Lot!.ClosingAt, b.Lot!.SellerId, b.Lot!.WinnerId))));
    }

    [Authorize]
    [HttpPost("{id}/recall")]
    public async Task<IResult> RecallBid([FromRoute] long id)
    {
        var hasId = long.TryParse(User.FindFirstValue("AccountId"), out var accountId);

        if (!hasId)
            return Results.Unauthorized();

        var result = await _bidService.RecallBidByIdAsync(id, accountId);

        if (result.IsBidUpdateError)
            return result.AsBidUpdateError switch
            {
                BidUpdateError.BidNotFound => Results.NotFound("Unknown bid"),
                BidUpdateError.BidLocked => Results.BadRequest("You have the top bid on this lot."),
                BidUpdateError.AccountNotFound => Results.NotFound("Unknown account"),
                BidUpdateError.LotNotOpen => Results.BadRequest("Lot is not open for bids"),
                BidUpdateError.LotClosed => Results.BadRequest("Lot is closed for bids"),
                BidUpdateError.NotOwner => Results.BadRequest("You are not the owner of this bid"),
                BidUpdateError.InvalidPrice => Results.BadRequest("Invalid price"),
                BidUpdateError.LotNotFound => Results.NotFound("Unknown lot"),
                _ => Results.StatusCode(500)
            };

        var bid = result.AsBid;

        return Results.Ok(new GetBidResponse(bid.Id, bid.LotId, bid.Price, bid.AccountId, bid.CreatedAt, bid.IsRecalled));
    }

    [Authorize]
    [HttpPost("recall")]
    public async Task<IResult> RecallBid([FromBody] RecallBidRequest request)
    {
        var hasId = long.TryParse(User.FindFirstValue("AccountId"), out var accountId);

        if (!hasId)
            return Results.Unauthorized();

        var result = await _bidService.RecallBidAsync(request.LotId, accountId);

        if (result.IsBidUpdateError)
            return result.AsBidUpdateError switch
            {
                BidUpdateError.BidNotFound => Results.NotFound("Unknown bid"),
                BidUpdateError.BidLocked => Results.BadRequest("You have the top bid on this lot."),
                BidUpdateError.AccountNotFound => Results.NotFound("Unknown account"),
                BidUpdateError.LotNotOpen => Results.BadRequest("Lot is not open for bids"),
                BidUpdateError.LotClosed => Results.BadRequest("Lot is closed for bids"),
                BidUpdateError.NotOwner => Results.BadRequest("You are not the owner of this bid"),
                BidUpdateError.InvalidPrice => Results.BadRequest("Invalid price"),
                BidUpdateError.LotNotFound => Results.NotFound("Unknown lot"),
                _ => Results.StatusCode(500)
            };

        var bid = result.AsBid;
        return Results.Ok(new GetBidResponse(bid.Id, bid.LotId, bid.Price, bid.AccountId, bid.CreatedAt, bid.IsRecalled));
    }

    [Authorize]
    [HttpPost("claim/{lotId}")]
    public async Task<IResult> ClaimBid([FromRoute] long lotId)
    {
        var hasId = long.TryParse(User.FindFirstValue("AccountId"), out var accountId);

        if (!hasId)
            return Results.Unauthorized();

        var result = await _bidService.Claim(lotId, accountId);

        if (result.IsBidClaimError)
            return result.AsBidClaimError switch
            {
                BidClaimError.NotFound => Results.NotFound("Unknown lot"),
                BidClaimError.NotClosed => Results.BadRequest("Lot is not closed"),
                BidClaimError.NotWinner => Results.BadRequest("You are not the winner of this lot"),
                BidClaimError.Claimed => Results.BadRequest("Lot is already claimed"),
                BidClaimError.NotOpen => Results.BadRequest("Lot is not closed. It's not even open to begin with :|"),
                _ => Results.StatusCode(500)
            };

        var lot = result.AsLot;
        return Results.Ok(new GetLotResponse(lot.Id, lot.SellerId));
    }

    public record CreateBidRequest(long LotId, long Amount);
    public record CreateBidResponse(long BidId, long LotId, long Amount, long AccountId, DateTimeOffset CreatedAt);
    public record GetBidResponse(long BidId, long LotId, long Amount, long AccountId, DateTimeOffset CreatedAt, bool IsRecalled = false);
    public record GetBidResponseWithLogin(long BidId, long LotId, long Amount, long AccountId, string Login, DateTimeOffset CreatedAt);
    public record GetBidResponseWithLot(long BidId, long LotId, long Amount, long AccountId, GetBidResponseLot responseLot);
    public record GetBidResponseLot(long LotId, string Title, long MinPrice, DateTimeOffset OpeningAt, DateTimeOffset ClosingAt, long SellerId, long? WinnerId);
    public record RecallBidRequest(long AccountId, long LotId);

    public record GetLotResponse(long LotId, long SellerId);

}
