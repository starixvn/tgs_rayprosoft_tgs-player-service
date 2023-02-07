using Microsoft.Extensions.Caching.Memory;
using Tgs.PlayerService;
using Tgs.PlayerService.ApplicationServices;
using Tgs.ServiceUtility;

namespace PlayerService.Core.Cache;

internal class GetCachedPlayerInfoByUserNameQueryService : IGetService<GetPlayerInfoByUserNameQuery, PlayerInfo?>
{
    private readonly IMemoryCache _memoryCache;
    private readonly IGetService<GetPlayerInfoByUserNameQuery, PlayerInfo?> _sourceService;

    public GetCachedPlayerInfoByUserNameQueryService(
        IGetService<GetPlayerInfoByUserNameQuery, PlayerInfo?> sourceService,
        IMemoryCache memoryCache)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByUserNameQuery query, CancellationToken cancellationToken = default)
    {
        var key = $"Player:UserName:{query.UserName.ToLower()}";

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
