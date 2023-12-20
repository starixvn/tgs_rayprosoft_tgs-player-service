using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerInfoByUserNameQueryService(IPlayerRepository playerRepository) : IGetService<GetPlayerInfoByUserNameQuery, PlayerInfo?>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByUserNameQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerInfoByUserNameAsync(query.BrandId, query.UserName, cancellationToken);
}
