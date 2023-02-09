using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class PlayerInfoListQueryService : IQueryService<PlayerInfoListQuery, PlayerInfo>
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerInfoListQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public IAsyncEnumerable<PlayerInfo> QueryAsync(
        PlayerInfoListQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository.QueryPlayerInfoListAsync(query.BrandId, query.PlayerCodes, cancellationToken);
}
