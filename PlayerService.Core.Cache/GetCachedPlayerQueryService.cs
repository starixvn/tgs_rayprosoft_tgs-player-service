using Microsoft.Extensions.Caching.Memory;
using Tgs.PlayerService;
using Tgs.PlayerService.ApplicationServices;
using Tgs.ServiceUtility;

namespace PlayerService.Core.Cache;

internal class GetCachedPlayerQueryService(
    IGetService<GetPlayerQuery, PlayerInfo?> sourceService,
    IMemoryCache memoryCache) : IGetService<GetPlayerQuery, PlayerInfo?>
{
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    private readonly IGetService<GetPlayerQuery, PlayerInfo?> _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));

    public async ValueTask<PlayerInfo?> GetAsync(GetPlayerQuery query, CancellationToken cancellationToken = default)
    {
        var key = $"Player:{query.PlayerCode}";

        return await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            var result = await _sourceService
                .GetAsync(query, cancellationToken)
                .ConfigureAwait(false);

            entry.AbsoluteExpiration = result is null
                ? DateTimeOffset.Now.AddSeconds(3D)
                : DateTimeOffset.Now.AddSeconds(10D);

            return result;
        }).ConfigureAwait(false);
    }
}
