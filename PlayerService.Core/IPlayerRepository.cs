using System.Collections.Immutable;

namespace Tgs.PlayerService;

public interface IPlayerRepository
{
    ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByEmailAsync(Guid brandId, string email, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByPhoneAsync(Guid brandId, string areaCode, string phone, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByUserNameAsync(Guid brandId, string userName, CancellationToken cancellationToken = default);

    ValueTask<IImmutableDictionary<string, string>> GetPlayerPropertiesAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerStatus> GetPlayerStatusAsync(string playerCode, CancellationToken cancellationToken = default);

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
}
