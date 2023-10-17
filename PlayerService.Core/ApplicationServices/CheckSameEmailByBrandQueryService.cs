using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class CheckSameEmailByBrandQueryService : IGetService<CheckSameEmailByBrandQuery, bool>

{
    private readonly IPlayerRepository _repository;

    public CheckSameEmailByBrandQueryService(IPlayerRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ValueTask<bool> GetAsync(CheckSameEmailByBrandQuery query, CancellationToken cancellationToken = default)
        => _repository.CheckSameEmailByBrandAsync(query.BrandId, query.PlayerCode, query.Email, cancellationToken);
}
