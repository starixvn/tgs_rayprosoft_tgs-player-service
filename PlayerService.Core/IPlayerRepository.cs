namespace Tgs.PlayerService.Core;

public interface IPlayerRepository
{
    ValueTask<PlayerInfo> GetPlayerInfoAsync(string playerCode);
}
