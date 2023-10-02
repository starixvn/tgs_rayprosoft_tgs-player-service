using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerEventQueryService : IGetService<GetPlayerEventQuery, EventInfo?>
{
    private readonly IPlayerRepository _repository;

    public GetPlayerEventQueryService(IPlayerRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ValueTask<EventInfo?> GetAsync(GetPlayerEventQuery query, CancellationToken cancellationToken = default)
        => _repository.GetPlayerEventAsync(query.EventId, cancellationToken);
}
