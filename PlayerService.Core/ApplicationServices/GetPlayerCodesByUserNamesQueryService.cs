using System.Collections.Immutable;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerCodesByUserNamesQueryService
    : IGetService<GetPlayerCodesByUserNamesQuery, IImmutableDictionary<string, string>>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerCodesByUserNamesQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public ValueTask<IImmutableDictionary<string, string>> GetAsync(
            GetPlayerCodesByUserNamesQuery query,
            CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerCodesByUserNamesAsync(query.BrandId, query.UserNames, cancellationToken);
}
