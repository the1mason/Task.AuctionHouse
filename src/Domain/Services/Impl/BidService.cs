using Domain.Models;
using Domain.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services.Impl;
public class BidService : IBidService
{
    private readonly TimeProvider _timeProvider;

    private readonly AuctionHouseContext _dbContext;

    private readonly ILotService _lotService;
    private readonly IAccountService _accountService;

    public BidService(TimeProvider timeProvider, ILotService lotService, AuctionHouseContext dbContext, IAccountService accountService)
    {
        _timeProvider = timeProvider;
        _lotService = lotService;
        _dbContext = dbContext;
        _accountService = accountService;
    }

    public async Task<Bid?> GetBidAsync(long id)
    {
        var result = await _dbContext.Bids.FirstOrDefaultAsync(x => x.Id == id);
        return result;
    }

    public async Task<Bid[]> GetBidsWithAccountsByLotAsync(long lotId, int limit, int offset, bool includeRecalled = true)
    {
        var query = _dbContext.Bids.Include(x => x.AccountTransaction).Include(x => x.Account).AsQueryable();
        query = query.Where(b => b.LotId == lotId);
        query = query.OrderByDescending(x => x.CreatedAt);

        if (!includeRecalled)
            query = query.Where(b => !b.IsRecalled);

        return await query.Skip(offset).Take(limit).ToArrayAsync();
    }

    public async Task<Bid[]> GetBidsWithLotsByAccountAsync(long accountId, int limit, int offset, bool includeRecalled = true)
    {
        var query = _dbContext.Bids.Include(x => x.AccountTransaction).Include(x => x.Lot).AsQueryable();
        query = query.Where(b => b.AccountId == accountId);
        query = query.OrderByDescending(x => x.CreatedAt);

        if (!includeRecalled)
            query = query.Where(b => !b.IsRecalled);

        return await query.Skip(offset).Take(limit).ToArrayAsync();
    }

    public async Task<Bid?> GetWinningBidWithAccount(long lotId)
    {
        var query = _dbContext.Bids.Include(x => x.AccountTransaction).Include(x => x.Account).AsQueryable();

        query = query.Where(b => b.LotId == lotId);

        query = query.OrderByDescending(b => b.Price);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<BidCreateResult> PlaceAsync(long lotId, long amount, long accountId)
    {
        var lot = await _lotService.GetLotAsync(lotId);
        var account = await _accountService.GetAccountAsync(accountId);
        var bid = await FindBidAsync(accountId, lotId);

        if (account is null)
            return BidCreateError.AccountNotFound;

        if (lot is null)
            return BidCreateError.LotNotFound;

        if (lot.IsClosed(_timeProvider))
            return BidCreateError.LotClosed;

        if (!lot.IsOpen(_timeProvider))
            return BidCreateError.LotNotOpen;

        if (lot.IsOwnedBy(accountId))
            return BidCreateError.CantBidSelf;

        var winningBid = await GetWinningBidWithAccount(lotId);
        if (winningBid != null && winningBid.Price >= amount)
            return BidCreateError.InvalidPrice;

        if (!account.CanAfford(amount))
            return BidCreateError.InsufficientFunds;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            if (bid is not null)
            {
                account.ReserveFunds(amount - bid.Price);

                bid.Price = amount;
                bid.CreatedAt = _timeProvider.GetUtcNow();
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return bid;
            }

            account.ReserveFunds(amount);
            var newBid = new Bid
            {
                LotId = lotId,
                AccountId = accountId,
                Price = amount,
                CreatedAt = _timeProvider.GetUtcNow()
            };

            _dbContext.Bids.Add(newBid);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return newBid;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<BidUpdateResult> RecallBidAsync(long lotId, long accountId)
    {
        var bid = await FindBidAsync(accountId, lotId);
        if (bid is null)
            return BidUpdateError.BidNotFound;
        return await RecallBidById(bid.Id, accountId);
    }

    public async Task<BidUpdateResult> RecallBidById(long id, long accountId)
    {
        var account = await _accountService.GetAccountAsync(accountId);
        var bid = await GetBidAsync(id);

        if (account is null)
            return BidUpdateError.AccountNotFound;

        if (bid is null)
            return BidUpdateError.BidNotFound;

        var winner = await GetWinningBidWithAccount(bid.LotId);

        if (winner != null && winner.Id == id)
            return BidUpdateError.BidLocked;

        if (bid.AccountId != accountId)
            return BidUpdateError.NotOwner;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            account.ReleaseFunds(bid.Price);
            bid.IsRecalled = true;
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return bid;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Bid?> FindBidAsync(long accountId, long lotId)
    {
        var result = await _dbContext.Bids.FirstOrDefaultAsync(x => x.AccountId == accountId && x.LotId == lotId);
        return result;
    }
}
