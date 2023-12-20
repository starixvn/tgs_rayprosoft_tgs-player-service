using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerCodesByUserNamesQueryService(
    IPlayerRepository playerRepository) : IQueryService<GetPlayerCodesByUserNamesQuery, string>
{
    public IAsyncEnumerable<string> QueryAsync(
        GetPlayerCodesByUserNamesQuery query,
        CancellationToken cancellationToken = default)
        => playerRepository.GetPlayerInfosByUserNamesAsync(
            query.BrandId,
            query.UserNames,
            cancellationToken)
            .Select(info => info.PlayerCode);
}
