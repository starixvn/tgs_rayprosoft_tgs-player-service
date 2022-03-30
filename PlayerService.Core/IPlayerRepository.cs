namespace Tgs.PlayerService.Core;

public interface IPlayerRepository
{
    ValueTask<decimal?> GetPlayerDepositedAmountAsync(string playerCode);

    ValueTask<PlayerDetailInfo> GetPlayerDetailAsync(string playerCode);

    ValueTask<PlayerInfo> GetPlayerInfoAsync(string playerCode);
}
