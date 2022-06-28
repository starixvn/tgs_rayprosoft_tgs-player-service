using Microsoft.Extensions.Caching.Memory;
using Tgs.PlayerService;
using Tgs.PlayerService.ApplicationServices;
using Tgs.ServiceUtility;

namespace PlayerService.Core.Cache;

internal class GetCachedPlayerInfoByPhoneQueryService : IGetService<GetPlayerInfoByPhoneQuery, PlayerInfo?>
{
    private readonly IMemoryCache _memoryCache;
    private readonly IGetService<GetPlayerInfoByPhoneQuery, PlayerInfo?> _sourceService;

    public GetCachedPlayerInfoByPhoneQueryService(IGetService<GetPlayerInfoByPhoneQuery, PlayerInfo?> sourceService, IMemoryCache memoryCache)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByPhoneQuery query, CancellationToken cancellationToken = default)
    {
        var key = $"Player:Phone:{query.AreaCode}:{query.Phone}";

        return await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            var result = await _sourceService
                .GetAsync(query, cancellationToken)
                .ConfigureAwait(false);

            if (result is null)
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(3D);
            else
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(10D);

            return result;
        }).ConfigureAwait(false);
    }
}
