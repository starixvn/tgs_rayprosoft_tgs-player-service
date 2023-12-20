using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

public class GetPlayerLoginInfoQueryService(IPlayerRepository playerRepository) : IGetService<GetPlayerLoginInfoQuery, PlayerLoginInfo?>
{
    public ValueTask<PlayerLoginInfo?> GetAsync(GetPlayerLoginInfoQuery query, CancellationToken cancellationToken = default)
        => playerRepository.GetPlayerLoginInfoAsync(query.PlayerCode, cancellationToken);
}
