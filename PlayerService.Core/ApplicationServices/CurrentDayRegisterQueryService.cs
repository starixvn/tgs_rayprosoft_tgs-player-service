using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class CurrentDayRegisterQueryService : IGetService<CurrentDayRegisterQuery, int>
{
    private readonly IPlayerRepository _playerRepository;

    public CurrentDayRegisterQueryService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<int> GetAsync(CurrentDayRegisterQuery query, CancellationToken cancellationToken = default)
        => _playerRepository.CurrentDayRegisterAsync(query.BrandId, cancellationToken);
}
