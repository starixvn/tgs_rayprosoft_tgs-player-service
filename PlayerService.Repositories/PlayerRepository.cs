using Microsoft.EntityFrameworkCore;
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

    public async ValueTask<IReadOnlyDictionary<string, string?>> GetPlayerExtraDataAsync(string playerCode)
    {
        var player = await _context.Players
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        var properties = new Dictionary<string, string?>();

        if (player == null)
            return properties;

        properties["RealName"] = player.RealName;
        properties["IdNumber"] = player.IdNumber;
        properties["Wechat"] = player.Wechat;
        properties["QQ"] = player.Qq;
        properties["AreaCode"] = player.AreaCode;
        properties["Phone"] = player.Phone;
        properties["Email"] = player.Email;

        return properties;
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

    public async ValueTask<IReadOnlyDictionary<string, string>> GetPlayerPropertiesAsync(Guid brandId, string playerCode)
        => await _context.PlayerProperties
            .Where(pp => pp.BrandId == brandId && pp.PlayerCode == playerCode)
            .ToDictionaryAsync(pp => pp.Name, pp => pp.Value)
            .ConfigureAwait(false);

    public async ValueTask SetPlayerPropertiesAsync(
        Guid brandId,
        string playerCode,
        IReadOnlyDictionary<string, string> properties)
    {
        var player = await _context.Players.FindAsync(playerCode).ConfigureAwait(false);

        if (player == null)
            return;

        var oldProperties = _context.PlayerProperties
            .Where(pp => pp.BrandId == brandId && pp.PlayerCode == playerCode);

        _context.PlayerProperties.RemoveRange(oldProperties);

        if (properties.Count > 0)
        {
            var newProperties = properties.Select(pp => new PlayerProperty
            {
                PlayerCode = playerCode,
                BrandId = brandId,
                Name = pp.Key,
                Value = pp.Value
            });

            await _context.PlayerProperties.AddRangeAsync(newProperties).ConfigureAwait(false);
        }

        player.UpdatedOn = DateTime.UtcNow;

        _ = await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}
