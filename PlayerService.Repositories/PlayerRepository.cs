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

    public async ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode)
    {
        var player = await _context.Players
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        return player?.AccumulatedDeposit ?? 0;
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

    public async ValueTask<IReadOnlyDictionary<string, string?>> GetPlayerPropertiesAsync(string playerCode)
    {
        var player = await _context.Players
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        var properties = new Dictionary<string, string?>();

        if (player == null)
        {
            return properties;
        }

        properties["RealName"] = player.RealName;
        properties["IdNumber"] = player.IdNumber;
        properties["Wechat"] = player.Wechat;
        properties["QQ"] = player.Qq;
        properties["AreaCode"] = player.AreaCode;
        properties["Phone"] = player.Phone;
        properties["Email"] = player.Email;

        return properties;
    }
}
