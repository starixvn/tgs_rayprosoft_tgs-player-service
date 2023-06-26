using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerCodesByUserNamesQueryService : IQueryService<GetPlayerCodesByUserNamesQuery, string>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerCodesByUserNamesQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public IAsyncEnumerable<string> QueryAsync(
        GetPlayerCodesByUserNamesQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerInfosByUserNamesAsync(
            query.BrandId,
            query.UserNames,
            cancellationToken)
            .Select(info => info.PlayerCode);
}
