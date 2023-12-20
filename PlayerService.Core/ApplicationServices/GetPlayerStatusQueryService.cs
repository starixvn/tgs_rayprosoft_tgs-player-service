using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerStatusQueryService(IPlayerRepository playerRepository) : IGetService<GetPlayerStatusQuery, PlayerStatus>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public ValueTask<PlayerStatus> GetAsync(GetPlayerStatusQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerStatusAsync(query.PlayerCode, cancellationToken);
}
