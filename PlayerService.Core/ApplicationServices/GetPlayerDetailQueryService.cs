using Tgs.ServiceUtility;

namespace Tgs.PlayerService.Core.ApplicationServices;

internal class GetPlayerDetailQueryService : IGetService<GetPlayerDetailQuery, PlayerDetailInfo>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerDetailQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<PlayerDetailInfo> GetAsync(
        GetPlayerDetailQuery query,
        CancellationToken cancellationToken = default) =>
        _playerRepository.GetPlayerDetailAsync(query.PlayerCode);
}
