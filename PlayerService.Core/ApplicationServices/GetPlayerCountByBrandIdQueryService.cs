using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerCountByBrandIdQueryService : IQueryService<GetPlayerCountByBrandIdQuery, PlayerCountInfo>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerCountByBrandIdQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public IAsyncEnumerable<PlayerCountInfo> QueryAsync(
        GetPlayerCountByBrandIdQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerCountByBrandIdAsync(query.BrandId, cancellationToken);
}
