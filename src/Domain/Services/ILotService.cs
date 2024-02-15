using Domain.Models;
using Domain.Services.Results;

namespace Domain.Services;
public interface ILotService
{
    public Task<Lot[]> GetLotsAsync(int limit, int offset, string title, long? minPrice, long? maxPrice);

    public Task<Lot> GetLotAsync(long id);

    public Task<Lot> CreateLotAsync(string title, string description, long minPrice, DateTimeOffset openingAt, DateTimeOffset closingAt, long sellerId);

    public Task<LotUpdateResult> UpdateLotAsync(long id, string title, string description, long minPrice, DateTimeOffset openingAt, DateTimeOffset closingAt);

    public Task<LotUpdateResult> DeleteLotAsync(long id);
}
