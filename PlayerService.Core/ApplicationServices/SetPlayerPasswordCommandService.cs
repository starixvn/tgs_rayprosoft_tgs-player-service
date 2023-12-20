using System.Text.Json;
using Json.Patch;
using Json.Pointer;
using Tgs.Core.Infrastructures.Security;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class SetPlayerPasswordCommandService(
    IPasswordHandler passwordHandler,
    IPlayerRepository playerRepository,
    ISerialNumberGenerator serialNumberGenerator) : ICommandService<SetPlayerPasswordCommand>
{
    private readonly IPasswordHandler _passwordHandler = passwordHandler ?? throw new ArgumentNullException(nameof(passwordHandler));
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    private readonly ISerialNumberGenerator _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));

    public async ValueTask ExecuteAsync(SetPlayerPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var (salt, password) = _passwordHandler.GenerateCredential(command.Password);

        await _playerRepository
            .SetPlayerPasswordAsync(command.PlayerCode, password, salt, command.ModifyUsername, cancellationToken)
            .ConfigureAwait(false);

        var patchOperations = Array.Empty<PatchOperation>();
        patchOperations = [
            .. patchOperations,
            PatchOperation.Replace(JsonPointer.Parse($"/password"), "")
        ];

        var changeSets = new JsonPatch(patchOperations);
        var eventId = await _serialNumberGenerator
            .GenerateAsync(cancellationToken)
            .ConfigureAwait(false);
        var currentTime = DateTimeOffset.UtcNow.UtcDateTime;

        var playerInfo = await _playerRepository
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        await _playerRepository.CreatePlayerEventAsync(
            eventId,
            playerInfo!.BrandId,
            command.PlayerCode,
            EventAction.Update,
            JsonSerializer.SerializeToDocument(changeSets),
            command.ModifyUserId,
            cancellationToken).ConfigureAwait(false);

        await _playerRepository.UpdatePlayerSnapshotAsync(
            command.PlayerCode,
            eventId,
            currentTime, cancellationToken).ConfigureAwait(false);
    }
}
