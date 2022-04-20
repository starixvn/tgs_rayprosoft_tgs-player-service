namespace Tgs.PlayerService.Client;

public interface IPlayerServiceClient
{
    ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode);

    ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode);

    ValueTask<IReadOnlyDictionary<string, string?>> GetPlayerPropertiesAsync(string playerCode);

    ValueTask SetPlayerPropertiesAsync(string playerCode, IReadOnlyDictionary<PlayerPropertyName, string?> properties);
}
