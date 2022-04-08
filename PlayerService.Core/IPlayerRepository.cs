namespace Tgs.PlayerService;

public interface IPlayerRepository
{
    ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode);

    ValueTask<PlayerExtraData?> GetPlayerExtraDataAsync(string playerCode);

    ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode);

    ValueTask<IReadOnlyDictionary<string, string>> GetPlayerPropertiesAsync(Guid brandId, string playerCode);

    ValueTask SetPlayerPropertiesAsync(
        Guid brandId,
        string playerCode,
        IReadOnlyDictionary<string, string> properties);
}
