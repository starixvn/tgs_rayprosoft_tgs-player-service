using Tgs.ServiceUtility;

namespace Tgs.PlayerService.Core.ApplicationServices;

internal class GetPlayerPropertiesQueryService : IGetService<GetPlayerPropertiesQuery, IReadOnlyDictionary<string, string?>>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerPropertiesQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<IReadOnlyDictionary<string, string?>> GetAsync(
        GetPlayerPropertiesQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerPropertiesAsync(query.PlayerCode);
}
