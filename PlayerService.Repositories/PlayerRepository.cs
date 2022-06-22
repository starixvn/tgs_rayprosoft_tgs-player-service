using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.Repositories;

internal class PlayerRepository : IPlayerRepository
{
    private readonly TgsContext _dbContext;

    public PlayerRepository(TgsContext context)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[] { playerCode }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return player?.AccumulatedDeposit ?? 0;
    }

    public async ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[] { playerCode }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return player == null
            ? null
            : new PlayerInfo(
                player.PlayerCode,
                player.BrandId,
                player.Username,
                player.IdentityAuthentication,
                player.PhoneAuthentication,
                !string.IsNullOrEmpty(player.Email),
                player.WithdrawPassword.Length > 0,
                player.VipId,
                player.RegisteredOn);
    }

    public async ValueTask<PlayerInfo?> GetPlayerInfoByEmailAsync(Guid brandId, string email, CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players.AsQueryable()
            .Where(player => player.BrandId == brandId
                && player.Email!.ToLower() == email.ToLower())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return player is null
            ? null
            : new PlayerInfo(
                player.PlayerCode,
                player.BrandId,
                player.Username,
                player.IdentityAuthentication,
                player.PhoneAuthentication,
                !string.IsNullOrEmpty(player.Email),
                player.WithdrawPassword.Length > 0,
                player.VipId,
                player.RegisteredOn);
    }

    public async ValueTask<PlayerInfo?> GetPlayerInfoByPhoneAsync(Guid brandId, string areaCode, string phone, CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players.AsQueryable()
            .Where(player => player.BrandId == brandId
                && player.AreaCode == areaCode
                && player.Phone == phone)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return player is null
            ? null
            : new PlayerInfo(
                player.PlayerCode,
                player.BrandId,
                player.Username,
                player.IdentityAuthentication,
                player.PhoneAuthentication,
                !string.IsNullOrEmpty(player.Email),
                player.WithdrawPassword.Length > 0,
                player.VipId,
                player.RegisteredOn);
    }

    public async ValueTask<PlayerInfo?> GetPlayerInfoByUserNameAsync(Guid brandId, string userName, CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players.AsQueryable()
            .Where(player => player.BrandId == brandId
                && player.Username!.ToLower() == userName.ToLower())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return player is null
            ? null
            : new PlayerInfo(
                player.PlayerCode,
                player.BrandId,
                player.Username,
                player.IdentityAuthentication,
                player.PhoneAuthentication,
                !string.IsNullOrEmpty(player.Email),
                player.WithdrawPassword.Length > 0,
                player.VipId,
                player.RegisteredOn);
    }

    public async ValueTask<IImmutableDictionary<string, string>> GetPlayerPropertiesAsync(
                    string playerCode,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[] { playerCode }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var properties = new Dictionary<string, string>();

        if (player == null)
            return properties.ToImmutableDictionary();

        var brandId = player.BrandId;

        if (!string.IsNullOrEmpty(player.RealName))
            properties["RealName"] = player.RealName;
        if (!string.IsNullOrEmpty(player.IdNumber))
            properties["IdNumber"] = player.IdNumber;
        if (!string.IsNullOrEmpty(player.Wechat))
            properties["Wechat"] = player.Wechat;
        if (!string.IsNullOrEmpty(player.Qq))
            properties["QQ"] = player.Qq;
        if (!string.IsNullOrEmpty(player.AreaCode))
            properties["AreaCode"] = player.AreaCode;
        if (!string.IsNullOrEmpty(player.Phone))
            properties["Phone"] = player.Phone;
        if (!string.IsNullOrEmpty(player.Email))
            properties["Email"] = player.Email;
        if (player.Birthdate != null)
            properties["Birthdate"] = player.Birthdate.ToString()!;

        properties["DefaultWalletId"] = player.DefaultWalletId.ToString();

        foreach (var prop in await _dbContext.PlayerProperties.AsQueryable()
            .Where(pp => pp.BrandId == brandId && pp.PlayerCode == playerCode)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false))
            properties[prop.Name] = prop.Value;

        return properties.ToImmutableDictionary();
    }

    public async ValueTask<PlayerStatus> GetPlayerStatusAsync(string playerCode, CancellationToken cancellationToken = default)
        => (PlayerStatus)(await _dbContext.Players
            .Where(player => player.PlayerCode == playerCode)
            .Select(player => (short?)player.Status)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false)
            ?? 1);

    public async ValueTask SaveLoginIpAsync(
        string playerCode,
        string ip,
        string? host,
        DateTime recordTime,
        CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.PlayerIpRecords
            .Where(x => x.PlayerCode == playerCode && x.IpType == 2)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            record = new PlayerIpRecord
            {
                PlayerCode = playerCode,
                IpType = 2,
                Ip = ip,
                Host = host,
                RecordOn = recordTime
            };

            _ = await _dbContext.PlayerIpRecords
                .AddAsync(record, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            record.Ip = ip;
            record.Host = host;
            record.RecordOn = recordTime;
        }

        _ = await _dbContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask SaveRegisterIpAsync(
        string playerCode,
        string ip,
        string? host,
        DateTime recordTime,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[] { playerCode }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
            return;

        player.RegisteredOn = recordTime;
        player.UpdatedByUsername = playerCode;
        player.UpdatedOn = DateTime.UtcNow;

        await _dbContext.PlayerIpRecords
            .AddRangeAsync(
                new[] {
                    new PlayerIpRecord
                    {
                        PlayerCode = playerCode,
                        IpType = 1,
                        Ip = ip,
                        Host = host,
                        RecordOn = recordTime
                    },
                    new PlayerIpRecord
                    {
                        PlayerCode = playerCode,
                        IpType = 2,
                        Ip = ip,
                        Host = host,
                        RecordOn = recordTime
                    }
                },
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _dbContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask SetPlayerPropertiesAsync(
                    Guid brandId,
        string playerCode,
        IEnumerable<IDataCommand> modifyCommands,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[] { playerCode }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (player == null)
            return;

        var context = new PlayerModifyCommandContext(
            player,
            _dbContext.PlayerProperties
                .Where(pp => pp.BrandId == brandId
                    && pp.PlayerCode == playerCode)
                .ToDictionary(prop => prop.Name),
            _dbContext);

        foreach (var command in modifyCommands)
            await command
                .ExecuteAsync(context, cancellationToken)
                .ConfigureAwait(false);

        player.UpdatedOn = DateTime.UtcNow;

        _ = await _dbContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
