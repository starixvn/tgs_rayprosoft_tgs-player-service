using System.Collections.Immutable;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerPropertiesQueryService : IGetService<GetPlayerPropertiesQuery, IImmutableDictionary<string, string>>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerPropertiesQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<IImmutableDictionary<string, string>> GetAsync(
        GetPlayerPropertiesQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository
            .GetPlayerPropertiesAsync(query.PlayerCode, cancellationToken);
}
