using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerInfoByEmailQueryService(IPlayerRepository playerRepository) : IGetService<GetPlayerInfoByEmailQuery, PlayerInfo?>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByEmailQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerInfoByEmailAsync(query.BrandId, query.Email, cancellationToken);
}
