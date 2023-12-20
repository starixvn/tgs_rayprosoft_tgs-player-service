using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class CountCurrentDayLoginQueryService(IPlayerRepository playerRepository) : IGetService<CountCurrentDayLoginQuery, int>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public ValueTask<int> GetAsync(CountCurrentDayLoginQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.CountCurrentDayLoginAsync(query.BrandId, cancellationToken);
}
