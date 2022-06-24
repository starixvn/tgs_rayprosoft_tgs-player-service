using System.Collections.Immutable;

namespace Tgs.PlayerService.Client;

public interface IPlayerServiceClient
{
    ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByEmailAsync(Guid brandId, string email, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByPhoneAsync(Guid brandId, string areaCode, string phone, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoByUserNameAsync(Guid brandId, string userName, CancellationToken cancellationToken = default);

    ValueTask<IImmutableDictionary<string, string>> GetPlayerPropertiesAsync(
        string playerCode,
        CancellationToken cancellationToken = default);

    ValueTask<PlayerStatus> GetPlayerStatusAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask RecordLoginIpAsync(string playerCode, string Ip, string? HostName, CancellationToken cancellationToken = default);

    ValueTask RecordRegisterIpAsync(string playerCode, string Ip, string? HostName, CancellationToken cancellationToken = default);

    ValueTask SetPlayerPropertiesAsync(
            string playerCode,
        IImmutableDictionary<PlayerPropertyName, string?> properties,
        CancellationToken cancellationToken = default);

    ValueTask SetPlayerStatusAsync(string playerCode, PlayerStatus playerStatus, CancellationToken cancellationToken = default);
}
