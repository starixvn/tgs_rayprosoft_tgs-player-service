namespace Tgs.PlayerService.Core;

public interface IPlayerRepository
{
    ValueTask<PlayerDetailInfo> GetPlayerDetailAsync(string playerCode);

    ValueTask<PlayerInfo> GetPlayerInfoAsync(string playerCode);
}
