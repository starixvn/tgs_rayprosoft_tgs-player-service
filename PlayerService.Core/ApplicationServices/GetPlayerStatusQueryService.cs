using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerStatusQueryService : IGetService<GetPlayerStatusQuery, PlayerStatus>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerStatusQueryService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<PlayerStatus> GetAsync(GetPlayerStatusQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerStatusAsync(query.PlayerCode, cancellationToken);
}
