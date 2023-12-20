using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class ListPlayerInfosByUserNameQueryService(IPlayerRepository playerRepository) : IQueryService<ListPlayerInfosByUserNameQuery, PlayerInfo>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public IAsyncEnumerable<PlayerInfo> QueryAsync(ListPlayerInfosByUserNameQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.QueryPlayerInfosByUserNameAsync(
            query.BrandId,
            query.UserNamePrefix,
            cancellationToken);
}
