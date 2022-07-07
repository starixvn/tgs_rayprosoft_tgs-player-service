using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerInfoByUserNameQueryService : IGetService<GetPlayerInfoByUserNameQuery, PlayerInfo?>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerInfoByUserNameQueryService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByUserNameQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerInfoByUserNameAsync(query.BrandId, query.UserName, cancellationToken);
}
