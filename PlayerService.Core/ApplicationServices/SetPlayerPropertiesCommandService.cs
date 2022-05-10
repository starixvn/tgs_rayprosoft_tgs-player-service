using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class SetPlayerPropertiesCommandService : ICommandService<SetPlayerPropertiesCommand>
{
    private readonly IDataCommandBuilder _dataCommandBuilder;
    private readonly IPlayerRepository _playerRepository;

    public SetPlayerPropertiesCommandService(
        IPlayerRepository playerRepository,
        IDataCommandBuilder dataCommandBuilder)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _dataCommandBuilder = dataCommandBuilder ?? throw new ArgumentNullException(nameof(dataCommandBuilder));
    }

    public async ValueTask ExecuteAsync(
        SetPlayerPropertiesCommand command,
        CancellationToken cancellationToken = default)
    {
        var player = await _playerRepository.GetPlayerInfoAsync(command.PlayerCode, cancellationToken);

        if (player == null)
            return;

        var availablePropertyNames = new HashSet<string>(
            Enum.GetNames<PlayerPropertyName>(),
            StringComparer.OrdinalIgnoreCase);
        var dataCommands = new List<IDataCommand>();

        foreach (var (key, value) in command.Properties
            .Select(kv => (
                KV: kv,
                RegularName: availablePropertyNames.TryGetValue(kv.Key, out var regularName)
                    ? regularName
                    : default))
            .Where(g => g.RegularName is not null)
            .ToDictionary(g => g.RegularName!, g => g.KV.Value))
        {
            if (value == null)
                dataCommands.Add(_dataCommandBuilder.CreateDeleteCommand(key));
            else
                dataCommands.Add(_dataCommandBuilder.CreatePutCommand(key, value));
        }

        await _playerRepository
            .SetPlayerPropertiesAsync(player.BrandId, player.PlayerCode, dataCommands, cancellationToken)
            .ConfigureAwait(false);
    }
}
