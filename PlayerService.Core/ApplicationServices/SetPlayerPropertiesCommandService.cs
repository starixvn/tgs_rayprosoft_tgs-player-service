using System.Text.Json;
using Json.Patch;
using Json.Pointer;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class SetPlayerPropertiesCommandService : ICommandService<SetPlayerPropertiesCommand>
{
    private readonly IDataCommandBuilder _dataCommandBuilder;
    private readonly IPlayerRepository _playerRepository;
    private readonly ISerialNumberGenerator _serialNumberGenerator;

    public SetPlayerPropertiesCommandService(
        IPlayerRepository playerRepository,
        IDataCommandBuilder dataCommandBuilder,
        ISerialNumberGenerator serialNumberGenerator)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _dataCommandBuilder = dataCommandBuilder ?? throw new ArgumentNullException(nameof(dataCommandBuilder));
        _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
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

        var patchOperations = Array.Empty<PatchOperation>();

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
            {
                dataCommands.Add(_dataCommandBuilder.CreateDeleteCommand(key));
                patchOperations = patchOperations.Append(PatchOperation.Remove(JsonPointer.Parse($"/{key}"))).ToArray();
            }
            else
            {
                dataCommands.Add(_dataCommandBuilder.CreatePutCommand(key, value));
                patchOperations = patchOperations.Append(PatchOperation.Replace(JsonPointer.Parse($"/{key}"), value)).ToArray();
            }
        }

        await _playerRepository
            .SetPlayerPropertiesAsync(player.BrandId, player.PlayerCode, dataCommands, cancellationToken)
            .ConfigureAwait(false);

        var changeSets = new JsonPatch(patchOperations);
        var eventId = await _serialNumberGenerator
            .GenerateAsync(cancellationToken)
            .ConfigureAwait(false);

        var currentTime = DateTimeOffset.UtcNow.UtcDateTime;

        await _playerRepository.CreatePlayerEventAsync(
            eventId,
            player.BrandId,
            player.PlayerCode,
            EventAction.Update,
            JsonSerializer.SerializeToDocument(changeSets),
            command.Modifier,
            cancellationToken).ConfigureAwait(false);

        await _playerRepository.UpdatePlayerSnapshotAsync(
            player.PlayerCode,
            eventId,
            currentTime, cancellationToken).ConfigureAwait(false);
    }
}
