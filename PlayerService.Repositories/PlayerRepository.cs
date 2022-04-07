using Microsoft.EntityFrameworkCore;
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

    public async ValueTask<PlayerExtraData?> GetPlayerExtraDataAsync(string playerCode)
    {
        var player = await _context.Players
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        return player == null
            ? null
            : new PlayerExtraData(
                player.RealName,
                player.IdNumber,
                player.Wechat,
                player.Qq,
                player.AreaCode,
                player.Phone,
                player.Email);
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
        var oldProperties = _context.PlayerProperties
            .Where(pp => pp.BrandId == brandId && pp.PlayerCode == playerCode);

        _context.PlayerProperties.RemoveRange(oldProperties);

        var newProperties = properties.Select(pp => new PlayerProperty
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            Name = pp.Key,
            Value = pp.Value
        });

        await _context.PlayerProperties.AddRangeAsync(newProperties).ConfigureAwait(false);
        _ = await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}
