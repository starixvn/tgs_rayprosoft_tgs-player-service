namespace Tgs.PlayerService.Core;

public interface IPlayerRepository
{
    ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode);

    ValueTask<PlayerExtraData?> GetPlayerExtraDataAsync(string playerCode);

    ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode);

    ValueTask<IReadOnlyDictionary<string, string>> GetPlayerPropertiesAsync(Guid brandId, string playerCode);
}
