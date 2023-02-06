using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetParentPlayerInfoQueryService : IGetService<GetParentPlayerInfoQuery, PlayerInfo?>
{
    private readonly IPlayerRepository _playerRepository;

    public GetParentPlayerInfoQueryService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async ValueTask<PlayerInfo?> GetAsync(GetParentPlayerInfoQuery query, CancellationToken cancellationToken = default)
    {
        var parentPlayerCode = await _playerRepository.GetParentPlayerCodeAsync(
            query.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        return string.IsNullOrEmpty(parentPlayerCode)
            ? null
            : await _playerRepository.GetPlayerInfoAsync(
            parentPlayerCode,
            cancellationToken).ConfigureAwait(false);
    }
}
