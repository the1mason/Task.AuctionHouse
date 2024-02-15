using Domain.Services;
using Domain.Services.Results;
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
    public async Task<IResult> GetBidsForLot(long id, int limit, int offset, bool includeRecalled = true)
    {

        if (limit < 0 || limit > 100)
            return Results.BadRequest();

        if (offset < 0)
            return Results.BadRequest();

        var bids = await _bidService.GetBidsWithAccountsByLotAsync(id, limit, offset, includeRecalled);

        return Results.Ok(bids.Select(b => new GetBidResponse(b.Id, b.LotId, b.Price, b.AccountId, b.CreatedAt)));
    }

    public record CreateBidRequest(long LotId, long Amount);
    public record CreateBidResponse(long BidId, long LotId, long Amount, long AccountId, DateTimeOffset CreatedAt);
    public record GetBidResponse(long BidId, long LotId, long Amount, long AccountId, DateTimeOffset CreatedAt);

}
