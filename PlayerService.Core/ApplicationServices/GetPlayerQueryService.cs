using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerQueryService(
    IPlayerRepository playerRepository) : IGetService<GetPlayerQuery, PlayerInfo?>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public ValueTask<PlayerInfo?> GetAsync(
        GetPlayerQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerInfoAsync(query.PlayerCode, cancellationToken);
}
