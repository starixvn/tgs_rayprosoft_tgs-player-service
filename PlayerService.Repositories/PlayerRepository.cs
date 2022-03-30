using Tgs.PlayerService.Core;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.Repositories;

internal class PlayerRepository : IPlayerRepository
{
    private readonly TgsContext _context;

    public PlayerRepository(
        TgsContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async ValueTask<decimal?> GetPlayerDepositedAmountAsync(string playerCode)
    {
        var player = await _context.Players
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        return player?.AccumulatedDeposit;
    }

    public async ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode)
    {
        var player = await _context.Players
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        return player == null
            ? null
            : new PlayerInfo(
                player.PlayerCode,
                player.BrandId,
                player.Username,
                player.IdentityAuthentication,
                player.PhoneAuthentication,
                (PlayerStatus)player.Status,
                player.WithdrawPassword.Length > 0,
                player.VipId);
    }

    public async ValueTask<IReadOnlyDictionary<string, string?>?> GetPlayerPropertiesAsync(string playerCode)
    {
        var player = await _context.Players
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        return player == null
            ? null
            : new Dictionary<string, string?>
            {
                ["RealName"] = player.RealName,
                ["IdNumber"] = player.IdNumber,
                ["Wechat"] = player.Wechat,
                ["QQ"] = player.Qq,
                ["AreaCode"] = player.AreaCode,
                ["Phone"] = player.Phone,
                ["Email"] = player.Email
            };
    }
}
