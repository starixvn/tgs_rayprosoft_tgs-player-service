using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class ListPlayerInfosByUserNamesQueryService : IQueryService<ListPlayerInfosByUserNamesQuery, PlayerInfo>
{
    private readonly IPlayerRepository _playerRepository;

    public ListPlayerInfosByUserNamesQueryService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public IAsyncEnumerable<PlayerInfo> QueryAsync(ListPlayerInfosByUserNamesQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerInfosByUserNamesAsync(
            query.BrandId,
            query.UserNames,
            cancellationToken);
}
