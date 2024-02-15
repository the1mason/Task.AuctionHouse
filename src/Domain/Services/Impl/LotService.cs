using Domain.Models;
using Domain.Services.Results;

namespace Domain.Services.Impl;
public class LotService : ILotService
{
    public async Task<Lot> GetLotAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<Lot[]> GetLotsAsync(int limit, int offset, string title, long? minPrice, long? maxPrice)
    {
        throw new NotImplementedException();
    }

    public async Task<Lot> CreateLotAsync(string title, string description, long minPrice, DateTimeOffset openingAt, DateTimeOffset closingAt, long sellerId)
    {
        throw new NotImplementedException();
    }
    public async Task<LotUpdateResult> UpdateLotAsync(long id, string title, string description, long minPrice, DateTimeOffset openingAt, DateTimeOffset closingAt)
    {
        throw new NotImplementedException();
    }

    public async Task<LotUpdateResult> DeleteLotAsync(long id)
    {
        throw new NotImplementedException();
    }




}
