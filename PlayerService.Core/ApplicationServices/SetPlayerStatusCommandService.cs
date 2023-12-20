using System.Text.Json;
using Json.Patch;
using Json.Pointer;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class SetPlayerStatusCommandService(
    IPlayerRepository playerRepository,
    ISystemClock systemClock,
    ISerialNumberGenerator serialNumberGenerator) : ICommandService<SetPlayerStatusCommand>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    private readonly ISerialNumberGenerator _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
    private readonly ISystemClock _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));

    public async ValueTask ExecuteAsync(SetPlayerStatusCommand command, CancellationToken cancellationToken = default)
    {
        await _playerRepository.SetPlayerStatusAsync(
            command.PlayerCode,
            command.Status,
            cancellationToken);

        var patchOperations = Array.Empty<PatchOperation>();

        patchOperations = [.. patchOperations, PatchOperation.Replace(JsonPointer.Parse("/status"), command.Status.ToString())];

        var changeSets = new JsonPatch(patchOperations);

        var eventId = await _serialNumberGenerator
            .GenerateAsync(cancellationToken)
            .ConfigureAwait(false);

        var currentTime = _systemClock.UtcNow.UtcDateTime;

        var playerInfo = await _playerRepository
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        await _playerRepository.CreatePlayerEventAsync(
            eventId,
            playerInfo!.BrandId,
            command.PlayerCode,
            EventAction.Update,
            JsonSerializer.SerializeToDocument(changeSets),
            command.Modifier,
            cancellationToken).ConfigureAwait(false);

        await _playerRepository.UpdatePlayerSnapshotAsync(
            command.PlayerCode,
            eventId,
            currentTime,
            cancellationToken).ConfigureAwait(false);
    }
}
