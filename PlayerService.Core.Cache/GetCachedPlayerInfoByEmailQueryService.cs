using Microsoft.Extensions.Caching.Memory;
using Tgs.PlayerService;
using Tgs.PlayerService.ApplicationServices;
using Tgs.ServiceUtility;

namespace PlayerService.Core.Cache;

internal class GetCachedPlayerInfoByEmailQueryService(
    IGetService<GetPlayerInfoByEmailQuery, PlayerInfo?> sourceService,
    IMemoryCache memoryCache) : IGetService<GetPlayerInfoByEmailQuery, PlayerInfo?>
{
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    private readonly IGetService<GetPlayerInfoByEmailQuery, PlayerInfo?> _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));

    public async ValueTask<PlayerInfo?> GetAsync(GetPlayerInfoByEmailQuery query, CancellationToken cancellationToken = default)
    {
        var key = $"Player:Email:{query.Email.ToLower()}";

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
