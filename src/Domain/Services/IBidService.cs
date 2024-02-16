using Domain.Models;
using Domain.Services.Results;

namespace Domain.Services;
public interface IBidService
{
    Task<BidCreateResult> PlaceAsync(long lotId, long amount, long accountId);

    Task<BidUpdateResult> RecallBidAsync(long lotId, long accountId);

    Task<BidUpdateResult> RecallBidByIdAsync(long id, long accountId);

    Task<Bid?> GetBidAsync(long id);

    Task<Bid?> FindBidAsync(long accountId, long lotId);

    Task<Bid[]> GetBidsWithAccountsByLotAsync(long lotId, int limit, int offset, bool includeRecalled = true);

    Task<Bid[]> GetBidsWithLotsByAccountAsync(long account, int limit, int offset, bool includeRecalled = true);

    Task<Bid?> GetWinningBidWithAccount(long lotId);
    public Task<BidClaimResult> Claim(long lotId, long accountId);

}
