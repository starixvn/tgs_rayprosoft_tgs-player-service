using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerInfoByPhoneQueryService : IGetService<GetPlayerInfoByPhoneQuery, PlayerInfo?>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerInfoByPhoneQueryService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByPhoneQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerInfoByPhoneAsync(
            query.BrandId,
            query.AreaCode,
            query.Phone,
            cancellationToken);
}
