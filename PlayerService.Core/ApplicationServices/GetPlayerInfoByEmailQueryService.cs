using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerInfoByEmailQueryService : IGetService<GetPlayerInfoByEmailQuery, PlayerInfo?>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerInfoByEmailQueryService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByEmailQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerInfoByEmailAsync(query.BrandId, query.Email, cancellationToken);
}
