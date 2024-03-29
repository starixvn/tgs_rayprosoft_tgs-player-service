using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using Json.Pointer;
using Tgs.CashService;
using Tgs.CashService.Client;
using Tgs.Core.Infrastructures.Security;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class CreatePlayerCommandService(
    IPlayerRepository playerRepository,
    IPasswordHandler passwordHandler,
    ISystemClock systemClock,
    ISerialNumberGenerator serialNumberGenerator,
    ICashServiceClient cashServiceClient,
    IDataCommandBuilder dataCommandBuilder) : IGetService<CreatePlayerCommand, string>
{
    private static readonly IReadOnlyDictionary<RegisterSiteType, PlayerTagType> _TagMapper =
        new Dictionary<RegisterSiteType, PlayerTagType>
        {
            [RegisterSiteType.Player] = PlayerTagType.PlayerCreated,
            [RegisterSiteType.Agent] = PlayerTagType.AgentCreated,
            [RegisterSiteType.BackendOffice] = PlayerTagType.BackendOfficeCreated
        }.AsReadOnly();

    private readonly ICashServiceClient _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    private readonly IDataCommandBuilder _dataCommandBuilder = dataCommandBuilder ?? throw new ArgumentNullException(nameof(dataCommandBuilder));
    private readonly IPasswordHandler _passwordHandler = passwordHandler ?? throw new ArgumentNullException(nameof(passwordHandler));
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    private readonly ISerialNumberGenerator _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
    private readonly ISystemClock _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));

    public async ValueTask<string> GetAsync(CreatePlayerCommand command, CancellationToken cancellationToken = default)
    {
        var (salt, password) = _passwordHandler.GenerateCredential(command.Password);

        var eventId = await _serialNumberGenerator.GenerateAsync(
            cancellationToken).ConfigureAwait(false);

        var createData = new CreatePlayerData(
            command.BrandId,
            command.Username,
            command.CurrencyCode,
            command.BetLimitId,
            PlayerStatus.RegisterUndone,
            command.PlayerType,
            command.VipId,
            command.AgentId,
            command.UpperPlayerCode,
            password,
            salt);

        var playerCode = await _playerRepository.CreatePlayerAsync(
            createData,
            cancellationToken).ConfigureAwait(false);

        await _playerRepository.CreatePlayerTagAsync(
            playerCode,
            _TagMapper[command.RegisterSiteType].ToString(),
            cancellationToken).ConfigureAwait(false);

        var createWalletId = await _cashServiceClient.CreateNewCashAsync(
            command.BrandId,
            playerCode,
            createData.CurrencyCode,
            CashType.Player,
            cancellationToken).ConfigureAwait(false);

        await _playerRepository.SetPlayerPropertiesAsync(
            createData.BrandId,
            playerCode,
            [
                _dataCommandBuilder.CreatePutCommand(
                    nameof(PlayerPropertyName.DefaultWalletId),
                    createWalletId.ToString())
            ],
            cancellationToken).ConfigureAwait(false);

        var currentTime = _systemClock.UtcNow.UtcDateTime;

        var createMainSnapshot = new
        {
            createData.BrandId,
            createData.AgentId,
            createData.VipId,
            createData.UserName,
            createData.CurrencyCode,
            createData.BetLimitId,
            createData.Type,
            Status = createData.PlayerStatus,
            DefaultWalletId = createWalletId
        };

        var patchOperations = Array.Empty<PatchOperation>();

        patchOperations = [
            .. patchOperations,
            PatchOperation.Add(
                JsonPointer.Parse(""),
                JsonNode.Parse(JsonSerializer.Serialize(
                    createMainSnapshot,
                    PlayerUtility.SerializerOptions))),
        ];

        if (!string.IsNullOrWhiteSpace(command.UpperPlayerCode))
            patchOperations = [
                .. patchOperations,
                PatchOperation.Replace(
                    JsonPointer.Parse("/parentPlayerCode"),
                    command.UpperPlayerCode),
            ];

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
