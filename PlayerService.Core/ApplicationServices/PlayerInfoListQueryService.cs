using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class PlayerInfoListQueryService(
    IPlayerRepository playerRepository)
    : IQueryService<PlayerInfoListQuery, PlayerInfo>
{
    public IAsyncEnumerable<PlayerInfo> QueryAsync(
        PlayerInfoListQuery query,
        CancellationToken cancellationToken = default)
        => playerRepository.QueryPlayerInfoListAsync(
            query.BrandId,
            query.PlayerCodes,
            query.Status,
            query.VipId,
            query.UserName,
            query.AgentId,
            query.ParentPlayerCode,
            query.BeginRegisterTime,
            query.EndRegisterTime,
            cancellationToken);
}
