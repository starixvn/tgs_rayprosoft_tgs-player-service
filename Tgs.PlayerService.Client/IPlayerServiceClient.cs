namespace Tgs.PlayerService.Client;

public interface IPlayerServiceClient
{
    ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyDictionary<string, string>> GetPlayerPropertiesAsync(
        string playerCode,
        CancellationToken cancellationToken = default);

    ValueTask SetPlayerPropertiesAsync(
        string playerCode,
        IReadOnlyDictionary<PlayerPropertyName, string?> properties,
        CancellationToken cancellationToken = default);
}
