using Tgs.ServiceUtility;

namespace Tgs.PlayerService.Core.ApplicationServices;

internal class GetPlayerQueryService : IGetService<GetPlayerQuery, PlayerInfo?>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<PlayerInfo?> GetAsync(
        GetPlayerQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerInfoAsync(query.PlayerCode);
}
