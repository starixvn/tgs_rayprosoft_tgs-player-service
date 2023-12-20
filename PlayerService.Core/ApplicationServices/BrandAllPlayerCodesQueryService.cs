using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class BrandAllPlayerCodesQueryService(IPlayerRepository playerRepository) : IQueryService<BrandAllPlayerCodesQuery, string>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public IAsyncEnumerable<string> QueryAsync(BrandAllPlayerCodesQuery query, CancellationToken cancellationToken = default)
    {
        return _playerRepository.QueryPlayerListWithoutLimitAsync(query.BrandId, cancellationToken)
            .Select(item => item.PlayerCode);
    }
}
