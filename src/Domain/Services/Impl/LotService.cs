using Domain.Models;
using Domain.Services.Results;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Domain.Services.Impl;
public class LotService : ILotService
{

    private readonly TimeProvider _timeProvider;

    private readonly AuctionHouseContext _dbContext;

    private readonly IPaymentService _paymentService;

    public LotService(TimeProvider timeProvider, AuctionHouseContext dbContext, IPaymentService paymentService)
    {
        _timeProvider = timeProvider;
        _dbContext = dbContext;
        _paymentService = paymentService;
    }

    public async Task<Lot?> GetLotAsync(long id)
    {
        return await _dbContext.Lots.Include(x => x.Seller).Include(x => x.Winner).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Lot[]> GetLotsAsync(int limit, int offset, string? title, long? minPrice, long? maxPrice,
        DateTimeOffset? opensAt, DateTimeOffset? closesAt, long? accountId, bool includeClosed = false, bool includeDeleted = false)
    {
        var query = _dbContext.Lots.Include(x => x.Seller).Include(x => x.Winner).AsQueryable();

        query = query.OrderBy(x => x.OpeningAt);

        if (!includeClosed)
            query = query.Where(l => l.ClosingAt > _timeProvider.GetUtcNow());

        if (!includeDeleted)
            query = query.Where(l => !l.IsDeleted);

        if (title != null)
            query = query.Where(l => l.Title.Contains(title));

        if (minPrice.HasValue)
            query = query.Where(l => l.MinPrice >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(l => l.MinPrice <= maxPrice);

        if (opensAt.HasValue)
            query = query.Where(l => l.OpeningAt >= opensAt);

        if (closesAt.HasValue)
            query = query.Where(l => l.ClosingAt <= closesAt);

        if (accountId.HasValue)
            query = query.Where(l => l.SellerId == accountId);

        return await query.Skip(offset).Take(limit).ToArrayAsync();
    }

    public async Task<LotCreateResult> CreateLotAsync(string title, string description, long minPrice, 
        DateTimeOffset openingAt, DateTimeOffset closingAt, long sellerId)
    {
        Lot newLot = new()
        {
            Title = title,
            Description = description,
            CreatedAt = _timeProvider.GetUtcNow(),
            SellerId = sellerId
        };

        try
        {
            newLot.SetOpeningAt(openingAt, _timeProvider);
        }
        catch (Exception)
        {
            return LotCreateError.InvalidCreatedAt;
        }

        try
        {
            newLot.SetClosingAt(closingAt);
        }
        catch (Exception)
        {
            return LotCreateError.InvalidClosedAt;
        }
        
        try
        {
            newLot.SetMinPice(minPrice);
        }
        catch (Exception)
        {
            return LotCreateError.InvalidPrice;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            _dbContext.Lots.Add(newLot);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return newLot;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<LotUpdateResult> UpdateLotAsync(long id, string title, string description, long minPrice, DateTimeOffset openingAt, DateTimeOffset closingAt, long accountId, Role role)
    {
        var query = _dbContext.Lots.AsQueryable();

        query = query.Where(x => x.IsDeleted == false);

        if (role < Role.Moderator)
            query = query.Where(x => x.SellerId == accountId);

        var lot = await query.FirstOrDefaultAsync(x => x.Id == id);

        if (lot == null)
            return LotUpdateError.NotFound;

        if (lot.IsClosed(_timeProvider))
            return LotUpdateError.AlreadyClosed;

        if (lot.IsOpen(_timeProvider))
            return LotUpdateError.AlreadyOpen;

        try
        {
            lot.SetOpeningAt(openingAt, _timeProvider);
        }
        catch (Exception)
        {
            return LotUpdateError.InvalidCreatedAt;
        }

        try
        {
            lot.SetClosingAt(closingAt);
        }
        catch (Exception)
        {
            return LotUpdateError.InvalidClosedAt;
        }
        try
        {
            lot.SetMinPice(minPrice);
        }
        catch (Exception)
        {
            return LotUpdateError.InvalidPrice;
        }


        lot.Title = title;
        lot.Description = description;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return lot;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<LotUpdateResult> DeleteLotAsync(long id, long accountId, Role role, bool force)
    {
        var query = _dbContext.Lots.AsQueryable();

        query = query.Where(x => x.IsDeleted == false);

        if (role < Role.Moderator)
            query = query.Where(x => x.SellerId == accountId);

        var lot = await query.FirstOrDefaultAsync(x => x.Id == id);

        if (lot == null)
            return LotUpdateError.NotFound;

        // Moderator can delete any lot
        bool canForce = force && role >= Role.Moderator;

        if (!canForce && lot.IsClosed(_timeProvider))
            return LotUpdateError.AlreadyClosed;

        if (!canForce && lot.IsOpen(_timeProvider))
            return LotUpdateError.AlreadyOpen;

        lot.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return lot;
    }
}
