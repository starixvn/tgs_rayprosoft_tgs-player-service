using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class CurrentDayRegisterQueryService(IPlayerRepository playerRepository) : IGetService<CurrentDayRegisterQuery, int>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public ValueTask<int> GetAsync(CurrentDayRegisterQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.CurrentDayRegisterAsync(query.BrandId, cancellationToken);
}
