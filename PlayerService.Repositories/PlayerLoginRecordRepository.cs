using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.Repositories;

internal class PlayerLoginRecordRepository(TgsContext context) : IPlayerLoginRecordRepository
{
    public IAsyncEnumerable<string> QueryGroupPlayerAsync(Guid brandId, DateTimeOffset startOn, DateTimeOffset endOn, int offset = 0, int limit = 100, CancellationToken cancellationToken = default)
        => context.PlayerLoginRecords
        .Where(predicate: plr => plr.CreatedOn >= startOn.UtcDateTime && plr.CreatedOn < endOn.UtcDateTime && plr.BrandId == brandId)
        .GroupBy(keySelector: plr => plr.PlayerCode)
        .OrderBy(keySelector: plr => plr.Key)
        .Select(selector: plr => plr.Key)
        .Skip(offset * limit)
        .Take(limit)
        .ToAsyncEnumerable();
}
