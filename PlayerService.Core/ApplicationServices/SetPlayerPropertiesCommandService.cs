using System.Text.Json;
using Json.Patch;
using Json.Pointer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class SetPlayerPropertiesCommandService(
    IPlayerRepository playerRepository,
    IDataCommandBuilder dataCommandBuilder,
    ISerialNumberGenerator serialNumberGenerator,
    IOptions<JsonOptions> jsonOptionsAccessor) : ICommandService<SetPlayerPropertiesCommand>
{
    private readonly IDataCommandBuilder _dataCommandBuilder = dataCommandBuilder ?? throw new ArgumentNullException(nameof(dataCommandBuilder));
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    private readonly JsonSerializerOptions _serializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
    private readonly ISerialNumberGenerator _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));

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

        var playerProperties = new string[]
        {
            nameof(PlayerPropertyName.Line),
            nameof(PlayerPropertyName.Viber),
            nameof(PlayerPropertyName.Telegram),
            nameof(PlayerPropertyName.FacebookMessenger),
            nameof(PlayerPropertyName.WhatsApp),
            nameof(PlayerPropertyName.Qq),
            nameof(PlayerPropertyName.Wechat),
            nameof(PlayerPropertyName.Zalo),
        };
        foreach (var (key, value) in command.Properties
            .Select(kv => (
                KV: kv,
                RegularName: availablePropertyNames.TryGetValue(kv.Key, out var regularName)
                    ? regularName
                    : default))
            .Where(g => g.RegularName is not null)
            .ToDictionary(g => g.RegularName!, g => g.KV.Value))
        {
            var source = $"/{key}";

            if (playerProperties.Contains(key))
                source = $"/properties/{key}";

            if (value == null)
            {
                dataCommands.Add(_dataCommandBuilder.CreateDeleteCommand(key));
                patchOperations = [.. patchOperations, PatchOperation.Remove(JsonPointer.Parse(source))];
            }
            else
            {
                dataCommands.Add(_dataCommandBuilder.CreatePutCommand(key, value));
                patchOperations = [.. patchOperations, PatchOperation.Replace(JsonPointer.Parse(source), value)];
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
            JsonSerializer.SerializeToDocument(changeSets, _serializerOptions),
            command.Modifier,
            cancellationToken).ConfigureAwait(false);

        await _playerRepository.UpdatePlayerSnapshotAsync(
            player.PlayerCode,
            eventId,
            currentTime, cancellationToken).ConfigureAwait(false);
    }
}
