using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class SetPlayerPropertiesCommandService : ICommandService<SetPlayerPropertiesCommand>
{
    private readonly IPlayerRepository _playerRepository;

    public SetPlayerPropertiesCommandService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async ValueTask ExecuteAsync(SetPlayerPropertiesCommand command,
        CancellationToken cancellationToken = default)
    {
        var player = await _playerRepository.GetPlayerInfoAsync(command.PlayerCode);

        if (player == null)
        {
            return;
        }

        var oldProperties = await _playerRepository
            .GetPlayerPropertiesAsync(player.BrandId, player.PlayerCode)
            .ConfigureAwait(false);

        var mergedProperties = new Dictionary<string, string>(oldProperties);

        foreach (var (key, value) in command.Properties)
        {
            if (value == null)
            {
                _ = mergedProperties.Remove(key);
            }
            else
            {
                mergedProperties[key] = value;
            }
        }

        await _playerRepository
            .SetPlayerPropertiesAsync(player.BrandId, player.PlayerCode, mergedProperties)
            .ConfigureAwait(false);
    }
}
