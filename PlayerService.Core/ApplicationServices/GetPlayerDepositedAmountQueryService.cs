using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerDepositedAmountQueryService : IGetService<GetPlayerDepositedAmountQuery, decimal>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerDepositedAmountQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask<decimal> GetAsync(
        GetPlayerDepositedAmountQuery query,
        CancellationToken cancellationToken = default)
        => _playerRepository.GetPlayerDepositedAmountAsync(query.PlayerCode, cancellationToken);
}
