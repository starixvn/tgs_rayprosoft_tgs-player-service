using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using Json.Pointer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Core.Infrastructures.Security;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class CreatePlayerCommandService : IGetService<CreatePlayerCommand, string>
{
    private readonly IPasswordHandler _passwordHandler;
    private readonly IPlayerRepository _playerRepository;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ISerialNumberGenerator _serialNumberGenerator;
    private readonly ISystemClock _systemClock;

    private readonly IImmutableDictionary<RegisterSiteType, PlayerTagType> _tagMapper =
        new Dictionary<RegisterSiteType, PlayerTagType>
        {
            [RegisterSiteType.Player] = PlayerTagType.PlayerCreated,
            [RegisterSiteType.Agent] = PlayerTagType.AgentCreated,
            [RegisterSiteType.BackendOffice] = PlayerTagType.BackendOfficeCreated
        }.ToImmutableDictionary();

    public CreatePlayerCommandService(
        IPlayerRepository playerRepository,
        IPasswordHandler passwordHandler,
        ISystemClock systemClock,
        ISerialNumberGenerator serialNumberGenerator,
        IOptions<JsonOptions> jsonOptionsAccessor)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _passwordHandler = passwordHandler ?? throw new ArgumentNullException(nameof(passwordHandler));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
        _serializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
    }

    public async ValueTask<string> GetAsync(CreatePlayerCommand command, CancellationToken cancellationToken = default)
    {
        var (salt, password) = _passwordHandler.GenerateCredential(command.Password);

        var eventId = await _serialNumberGenerator
            .GenerateAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerCode = await _playerRepository
            .CreatePlayerAsync(
                new CreatePlayerData
                {
                    BrandId = command.BrandId,
                    AgentId = command.AgentId,
                    VipId = command.VipId,
                    UserName = command.Username,
                    Salt = salt,
                    Password = password,
                    CurrencyCode = command.CurrencyCode,
                    BetLimitId = command.BetLimitId,
                    Type = command.PlayerType,
                    PlayerStatus = PlayerStatus.RegisterUndone,
                    UpperPlayerCode = command.UpperPlayerCode
                }, cancellationToken)
            .ConfigureAwait(false);

        await _playerRepository
            .CreatePlayerTagAsync(playerCode, _tagMapper[command.RegisterSiteType].ToString(), cancellationToken)
            .ConfigureAwait(false);

        var currentTime = _systemClock.UtcNow.UtcDateTime;

        var createMainSnapshot = new
        {
            command.BrandId,
            command.AgentId,
            command.VipId,
            command.Username,
            command.CurrencyCode,
            command.BetLimitId,
            Type = command.PlayerType,
            Status = PlayerStatus.RegisterUndone,
        };

        var patchOperations = Array.Empty<PatchOperation>();

        patchOperations = patchOperations.Append(PatchOperation.Add(JsonPointer.Parse(""), JsonNode.Parse(JsonSerializer.Serialize(createMainSnapshot, _serializerOptions)))).ToArray();

        if (!string.IsNullOrWhiteSpace(command.UpperPlayerCode))
        {
            patchOperations = patchOperations.Append(PatchOperation.Replace(JsonPointer.Parse("/parentPlayerCode"), command.UpperPlayerCode)).ToArray();
        }

        var changeSets = new JsonPatch(patchOperations);

        await _playerRepository.CreatePlayerEventAsync(
            eventId,
            command.BrandId,
            playerCode,
            EventAction.Create,
            JsonSerializer.SerializeToDocument(changeSets),
            modifier: null,
            cancellationToken).ConfigureAwait(false);

        await _playerRepository.UpdatePlayerSnapshotAsync(
            playerCode,
            eventId,
            currentTime,
            cancellationToken).ConfigureAwait(false);

        return playerCode;
    }
}
