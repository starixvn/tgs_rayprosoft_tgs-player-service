using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetAgentIdForPlayerUpgradedQueryService(IPlayerRepository playerRepository) : IGetService<GetAgentIdForPlayerUpgradedQuery, Guid?>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public ValueTask<Guid?> GetAsync(GetAgentIdForPlayerUpgradedQuery query, CancellationToken cancellationToken = default)
    {
        return _playerRepository.GetAgentIdForPlayerUpgradedAsync(
            query.PlayerCode,
            cancellationToken);
    }
}
