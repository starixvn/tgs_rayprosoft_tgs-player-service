using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerCountByBrandIdQueryService(
    IPlayerRepository playerRepository) : IQueryService<GetPlayerCountByBrandIdQuery, PlayerCountInfo>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public IAsyncEnumerable<PlayerCountInfo> QueryAsync(
        GetPlayerCountByBrandIdQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerCountByBrandIdAsync(query.BrandId, cancellationToken);
}
