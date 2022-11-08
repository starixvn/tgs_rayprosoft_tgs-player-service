using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

public class GetPlayerLoginInfoQueryService : IGetService<GetPlayerLoginInfoQuery, PlayerLoginInfo?>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerLoginInfoQueryService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<PlayerLoginInfo?> GetAsync(GetPlayerLoginInfoQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerLoginInfoAsync(query.PlayerCode, cancellationToken);
}
