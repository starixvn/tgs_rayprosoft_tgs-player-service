using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class PlayerListQueryService : IQueryService<PlayerListQuery, PlayerListItem>
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerListQueryService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public IAsyncEnumerable<PlayerListItem> QueryAsync(PlayerListQuery query, CancellationToken cancellationToken = default)
        => query.PlayerCodes.Any()
            ? _playerRepository.QueryPlayerListByPlayersAsync(
                query.BrandId,
                query.PlayerCodes,
                query.Status,
                query.VipId,
                query.UserName,
                query.AgentId,
                query.ParentPlayerCode,
                query.Phone,
                query.RealName,
                query.RegisteredDomainName,
                query.BeginRegisterTime,
                query.EndRegisterTime,
                cancellationToken)
            : _playerRepository.QueryPlayerListAsync(
                query.BrandId,
                query.Status,
                query.VipId,
                query.UserName,
                query.AgentId,
                query.ParentPlayerCode,
                query.Phone,
                query.RealName,
                query.RegisteredDomainName,
                query.BeginRegisterTime,
                query.EndRegisterTime,
                cancellationToken);
}
