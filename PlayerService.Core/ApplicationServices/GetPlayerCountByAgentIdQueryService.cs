using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerCountByAgentIdQueryService : IGetService<GetPlayerCountByAgentIdQuery, int>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerCountByAgentIdQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<int> GetAsync(
        GetPlayerCountByAgentIdQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerCountByAgentIdsAsync(query.AgentIds, cancellationToken);
}
