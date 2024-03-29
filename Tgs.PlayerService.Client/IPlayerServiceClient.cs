using System.Collections.Immutable;

namespace Tgs.PlayerService.Client;

public interface IPlayerServiceClient
{
    ValueTask<int> CountCurrentDayLoginPlayersAsync(Guid brandId, CancellationToken cancellationToken = default);

    ValueTask<int> CountCurrentDayRegisterPlayersAsync(Guid brandId, CancellationToken cancellationToken = default);

    ValueTask<int> CountPlayerHistoriesAsync(GetPlayerHistoriesQueryConditions conditions, CancellationToken cancellationToken = default);

    ValueTask<int> CountReportDateRegisterPlayersAsync(Guid brandId, DateOnly reportDate, CancellationToken cancellationToken = default);

    ValueTask<string> CreatePlayerAsync(CreatePlayerData data, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetParentPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetPlayerCodesByUserNamesAsync(
        Guid brandId,
        IEnumerable<string> userNames,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerCountInfo> GetPlayerCountInfosByBrandIdAsync(Guid brandId,
        CancellationToken cancellationToken = default);

    ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode, CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerHistoryInfo> GetPlayerHistoriesAsync(
        GetPlayerHistoriesQueryConditions conditions,
        int totalCount,
        int skip = 0,
        CancellationToken cancellationToken = default);

    ValueTask<PlayerEventInfo?> GetPlayerHistoryAsync(Guid eventId, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByEmailAsync(Guid brandId, string email, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByPhoneAsync(
        Guid brandId,
        string areaCode,
        string phone,
        CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByUserNameAsync(Guid brandId, string userName, CancellationToken cancellationToken = default);

    ValueTask<PlayerLoginInfo?> GetPlayerLoginInfoAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<IImmutableDictionary<string, string>> GetPlayerPropertiesAsync(
        string playerCode,
        CancellationToken cancellationToken = default);

    ValueTask<PlayerStatus> GetPlayerStatusAsync(string playerCode, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ListPlayerCodesByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<KeyValuePair<string, string>> ListPlayerCodesByUserNameAsync(
        Guid brandId,
        string userNamePrefix,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<KeyValuePair<string, string>> ListPlayerCodesByUserNamesAsync(
                                                    Guid brandId,
        IEnumerable<string> userNames,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerInfo> QueryPlayerInfoListAsync(
        Guid brandId,
        QueryPlayerListConditions conditions,
        CancellationToken cancellationToken = default);

    [Obsolete("淘汰API")]
    IAsyncEnumerable<PlayerListItem> QueryPlayerListAsync(
        Guid brandId,
        PlayerListQuery query,
        CancellationToken cancellationToken = default);

    ValueTask RecordLoginIpAsync(string playerCode, string ip, string? hostName, CancellationToken cancellationToken = default);

    ValueTask RecordRegisterIpAsync(string playerCode, string ip, string? hostName, CancellationToken cancellationToken = default);

    ValueTask SetPlayerPasswordAsync(
        string playerCode,
        string password,
        string? modifyUsername,
        string? modifyUserId,
        CancellationToken cancellationToken = default);

    ValueTask SetPlayerPropertiesAsync(
        string playerCode,
        IImmutableDictionary<PlayerPropertyName, string?> properties,
        string? modifyUser = null,
        CancellationToken cancellationToken = default);

    ValueTask SetPlayerStatusAsync(
        string playerCode,
        PlayerStatus playerStatus,
        string? modifyUser = null,
        CancellationToken cancellationToken = default);

    ValueTask TopUpAccumulatedDepositAsync(string playerCode, decimal amount, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> QueryPlayerBirthdayAsync(Guid brandId, DateOnly birthday, int page = 1, int rows = 100, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> QueryPlayerLoginRecordGroupPlayerAsync(Guid brandId, DateTimeOffset startOn, DateTimeOffset endOn, int page = 1, int rows = 100, CancellationToken cancellationToken = default);

    ValueTask UpgradePlayerToAgentAsync(
        Guid brandId,
        Guid agentId,
        string playerCode,
        string? modifyUser = null,
        CancellationToken cancellationToken = default);

    ValueTask<Guid?> GetAgentIdForPlayerUpgradedAsync(string playerCode, CancellationToken cancellationToken = default);
}
