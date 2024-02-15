using Domain.Models;
using Domain.Services.Results;
using Microsoft.EntityFrameworkCore.Query;

namespace Domain.Services;
public interface ILotService
{
    public Task<Lot[]> GetLotsAsync(int limit, int offset, string? title, long? minPrice, long? maxPrice,
        DateTimeOffset? opensAt, DateTimeOffset? closesAt, long? accountId, bool includeClosed = false, bool includeDeleted = false);

    public Task<Lot?> GetLotAsync(long id);

    public Task<LotCreateResult> CreateLotAsync(string title, string description, long minPrice,
        DateTimeOffset openingAt, DateTimeOffset closingAt, long sellerId);

    public Task<LotUpdateResult> UpdateLotAsync(long id, string title, string description, long minPrice, DateTimeOffset openingAt, DateTimeOffset closingAt, long accountId, Role role);

    public Task<LotUpdateResult> DeleteLotAsync(long id, long accountId, Role role, bool force);

    public Task<LotClaimResult> Claim(long id, long accountId);
}
