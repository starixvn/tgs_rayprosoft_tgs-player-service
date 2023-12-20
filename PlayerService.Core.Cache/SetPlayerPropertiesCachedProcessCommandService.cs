using Microsoft.Extensions.Caching.Memory;
using Tgs.PlayerService.ApplicationServices;
using Tgs.ServiceUtility;

namespace PlayerService.Core.Cache;

internal class SetPlayerPropertiesCachedProcessCommandService(
    ICommandService<SetPlayerPropertiesCommand> sourceService,
    IMemoryCache memoryCache) : ICommandService<SetPlayerPropertiesCommand>
{
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    private readonly ICommandService<SetPlayerPropertiesCommand> _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));

    public async ValueTask ExecuteAsync(SetPlayerPropertiesCommand command, CancellationToken cancellationToken = default)
    {
        var key = $"PlayerProperties:{command.PlayerCode}";

        await _sourceService
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);

        _memoryCache.Remove(key);
    }
}
