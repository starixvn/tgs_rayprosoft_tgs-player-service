using System.Collections.Immutable;

namespace Tgs.PlayerService;

public interface IPlayerRepository
{
    ValueTask AddPlayerAuditLogAsync(
        Guid createId,
        string userName,
        string moduleName,
        string operation,
        string modifier,
        CancellationToken cancellationToken = default);

    ValueTask<string> CreatePlayerAsync(CreatePlayerData data, CancellationToken cancellationToken = default);

    ValueTask CreatePlayerTagAsync(string playerCode, string tag, CancellationToken cancellationToken = default);

    ValueTask<string?> GetParentPlayerCodeAsync(string playerCode, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetPlayerCodesByUserNamesAsync(
        Guid brandId,
        IEnumerable<string> userNames,
        CancellationToken cancellationToken = default);

    ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByEmailAsync(Guid brandId, string email, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByPhoneAsync(Guid brandId, string areaCode, string phone, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByUserNameAsync(Guid brandId, string userName, CancellationToken cancellationToken = default);

    ValueTask<PlayerLoginInfo?> GetPlayerLoginInfoAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<IImmutableDictionary<string, string>> GetPlayerPropertiesAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerStatus> GetPlayerStatusAsync(string playerCode, CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerInfo> QueryPlayerInfoListAsync(
        IEnumerable<string> playerCodes, 
        CancellationToken cancellationToken);

    IAsyncEnumerable<PlayerListItem> QueryPlayerListAsync(
        Guid brandId,
        PlayerStatus? status = null,
        Guid? vipId = null,
        string? userName = null,
        Guid? agentId = null,
        string? parentPlayerCode = null,
        string? phone = null,
        string? realName = null,
        string? registeredDomainName = null,
        DateTime? beginRegisterTime = null,
        DateTime? endRegisterTime = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerListItem> QueryPlayerListByPlayersAsync(
        Guid brandId,
        IEnumerable<string> playerCodes,
        PlayerStatus? status = null,
        Guid? vipId = null,
        string? userName = null,
        Guid? agentId = null,
        string? parentPlayerCode = null,
        string? phone = null,
        string? realName = null,
        string? registeredDomainName = null,
        DateTime? beginRegisterTime = null,
        DateTime? endRegisterTime = null,
        CancellationToken cancellationToken = default);

    ValueTask SaveLoginIpAsync(
        string playerCode,
        string ip,
        string? host,
        DateTime recordTime,
        CancellationToken cancellationToken = default);

    ValueTask SaveRegisterIpAsync(
        string playerCode,
        string ip,
        string? host,
        DateTime recordTime,
        CancellationToken cancellationToken = default);

    ValueTask SetPlayerPropertiesAsync(
        Guid brandId,
        string playerCode,
        IEnumerable<IDataCommand> modifyCommands,
        CancellationToken cancellationToken = default);

    ValueTask SetPlayerStatusAsync(string playerCode, PlayerStatus status, CancellationToken cancellationToken = default);

    ValueTask TopUpAccumulatedDepositAsync(string playerCode, decimal amount, CancellationToken cancellationToken = default);
}
