using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Tgs.PlayerService.ApplicationServices;
using Tgs.ServiceUtility;

namespace PlayerService.Core.Cache;

internal class GetCachedPlayerPropertiesQueryService : IGetService<GetPlayerPropertiesQuery, IImmutableDictionary<string, string>>
{
    private readonly IMemoryCache _memoryCache;
    private readonly IGetService<GetPlayerPropertiesQuery, IImmutableDictionary<string, string>> _sourceService;

    public GetCachedPlayerPropertiesQueryService(
        IGetService<GetPlayerPropertiesQuery, IImmutableDictionary<string, string>> sourceService,
        IMemoryCache memoryCache)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async ValueTask<IImmutableDictionary<string, string>> GetAsync(
        GetPlayerPropertiesQuery query,
        CancellationToken cancellationToken = default)
    {
        var key = $"PlayerProperties:{query.PlayerCode}";

        return (await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(10D);

            return await _sourceService
                .GetAsync(query, cancellationToken)
                .ConfigureAwait(false);
        }).ConfigureAwait(false))!;
    }
}
