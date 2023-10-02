using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerHistoriesQueryService
    : IQueryService<GetPlayerHistoriesQuery, PlayerHistoryInfo>, IGetService<GetPlayerHistoriesQuery, int>
{
    private readonly IPlayerRepository _repository;

    public GetPlayerHistoriesQueryService(IPlayerRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ValueTask<int> GetAsync(GetPlayerHistoriesQuery query, CancellationToken cancellationToken = default)
        => _repository.CountPlayerEventAsync(new PlayerEventQueryConditions(query.PlayerCode, query.From, query.To), cancellationToken);

    public IAsyncEnumerable<PlayerHistoryInfo> QueryAsync(GetPlayerHistoriesQuery query, CancellationToken cancellationToken = default)
        => _repository.QueryPlayerEventAsync(new PlayerEventQueryConditions(
            query.PlayerCode,
            query.From,
            query.To),
            query.Paging,
            cancellationToken);
}
