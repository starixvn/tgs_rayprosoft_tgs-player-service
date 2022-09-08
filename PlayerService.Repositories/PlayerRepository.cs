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

    public IAsyncEnumerable<PlayerListItem> QueryPlayerListAsync(
        Guid brandId,
        PlayerStatus? status = null,
        Guid? vipId = null,
        string? userName = null,
        Guid? agentId = null,
        string? parentPlayerCode = null,
        string? phone = null,
        string? realName = null,
        string? registeredDomainName = null,
        DateTime? beginRegisterTime = null,
        DateTime? endRegisterTime = null,
        CancellationToken cancellationToken = default)
    {
        var players = InternalQueryPlayerList(
            brandId,
            status,
            vipId,
            userName,
            agentId,
            phone,
            realName,
            beginRegisterTime,
            endRegisterTime);

        var query = (from player in players
                        .Select(p => new
                        {
                            p.PlayerCode,
                            p.Username,
                            p.RealName,
                            p.RegisteredOn,
                            p.AgentId,
                            p.VipId,
                            p.Status
                        })
                     join parent in _dbContext.PlayerParents
                         on player.PlayerCode equals parent.PlayerCode into pg
                     from parent in pg.DefaultIfEmpty()
                     join ipRecord in _dbContext.PlayerIpRecords
                         on new { player.PlayerCode, IpType = (short)PlayerIpType.Register } equals new { ipRecord.PlayerCode, ipRecord.IpType } into pirg
                     from ipRecord in pirg.DefaultIfEmpty()
                     select new PlayerListItem(
                         player.PlayerCode,
                         player.Username,
                         player.RealName,
                         player.RegisteredOn,
                         ipRecord.Host,
                         player.AgentId,
                         player.VipId,
                         (PlayerStatus)player.Status,
                         parent.ParentPlayerCode)).ToAsyncEnumerable();

        if (registeredDomainName is not null)
            query = query.Where(item => string.Equals(
                item.RegisteredDomainName,
                registeredDomainName,
                StringComparison.OrdinalIgnoreCase));

        if (parentPlayerCode is not null)
            query = query.Where(item => item.ParentPlayerCode == parentPlayerCode);

        return query;
    }

    public IAsyncEnumerable<PlayerListItem> QueryPlayerListByPlayersAsync(
        Guid brandId,
        IEnumerable<string> playerCodes,
        PlayerStatus? status = null,
        Guid? vipId = null,
        string? userName = null,
        Guid? agentId = null,
        string? parentPlayerCode = null,
        string? phone = null,
        string? realName = null,
        string? registeredDomainName = null,
        DateTime? beginRegisterTime = null,
        DateTime? endRegisterTime = null,
        CancellationToken cancellationToken = default)
    {
        var players = InternalQueryPlayerList(
            brandId,
            status,
            vipId,
            userName,
            agentId,
            phone,
            realName,
            beginRegisterTime,
            endRegisterTime).Where(player => playerCodes.Contains(player.PlayerCode));

        var query = (from player in players
                        .Select(p => new
                        {
                            p.PlayerCode,
                            p.Username,
                            p.RealName,
                            p.RegisteredOn,
                            p.AgentId,
                            p.VipId,
                            p.Status
                        })
                     join parent in _dbContext.PlayerParents
                         on player.PlayerCode equals parent.PlayerCode into pg
                     from parent in pg.DefaultIfEmpty()
                     join ipRecord in _dbContext.PlayerIpRecords
                         on new { player.PlayerCode, IpType = (short)PlayerIpType.Register } equals new { ipRecord.PlayerCode, ipRecord.IpType } into pirg
                     from ipRecord in pirg.DefaultIfEmpty()
                     select new PlayerListItem(
                         player.PlayerCode,
                         player.Username,
                         player.RealName,
                         player.RegisteredOn,
                         ipRecord.Host,
                         player.AgentId,
                         player.VipId,
                         (PlayerStatus)player.Status,
                         parent.ParentPlayerCode)).ToAsyncEnumerable();

        if (registeredDomainName is not null)
            query = query.Where(item => string.Equals(
                item.RegisteredDomainName,
                registeredDomainName,
                StringComparison.OrdinalIgnoreCase));

        if (parentPlayerCode is not null)
            query = query.Where(item => item.ParentPlayerCode == parentPlayerCode);

        return query;
    }

    public async ValueTask SaveLoginIpAsync(
        string playerCode,
        string ip,
        string? host,
        DateTime recordTime,
        CancellationToken cancellationToken = default)
    {
        var recordTimeUtc = recordTime.ToUniversalTime();

        var record = await _dbContext.PlayerIpRecords
            .Where(x => x.PlayerCode == playerCode && x.IpType == 2)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false)
            ?? (await _dbContext.PlayerIpRecords
                .AddAsync(new PlayerIpRecord
                {
                    PlayerCode = playerCode,
                    IpType = 2,
                    Ip = ip,
                    Host = host,
                    RecordOn = recordTime
                }, cancellationToken)
                .ConfigureAwait(false)).Entity;

        record.Ip = ip;
        record.Host = host;
        record.RecordOn = recordTime;

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

        var recordTimeUtc = recordTime.ToUniversalTime();

        player.RegisteredOn = recordTimeUtc;
        player.UpdatedByUsername = playerCode;
        player.UpdatedOn = DateTime.UtcNow;

        var loginRecord = await _dbContext.PlayerIpRecords
            .Where(x => x.PlayerCode == playerCode && x.IpType == 2)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false)
            ?? (await _dbContext.PlayerIpRecords
                .AddAsync(new PlayerIpRecord
                {
                    PlayerCode = playerCode,
                    IpType = 2,
                    Ip = ip,
                    Host = host,
                    RecordOn = recordTimeUtc
                }, cancellationToken)
                .ConfigureAwait(false)).Entity;

        loginRecord.Ip = ip;
        loginRecord.Host = host;
        loginRecord.RecordOn = recordTimeUtc;

        _ = await _dbContext.PlayerIpRecords
            .AddAsync(
                new PlayerIpRecord
                {
                    PlayerCode = playerCode,
                    IpType = 1,
                    Ip = ip,
                    Host = host,
                    RecordOn = recordTimeUtc
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

    public async ValueTask SetPlayerStatusAsync(string playerCode, PlayerStatus status, CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[] { playerCode }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (player == null)
            return;

        player.Status = (short)status;
        player.UpdatedOn = DateTime.UtcNow;

        _ = await _dbContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private IQueryable<Player> InternalQueryPlayerList(
                            Guid brandId,
        PlayerStatus? status = null,
        Guid? vipId = null,
        string? userName = null,
        Guid? agentId = null,
        string? phone = null,
        string? realName = null,
        DateTime? beginRegisterTime = null,
        DateTime? endRegisterTime = null)
    {
        var players = _dbContext.Players.AsQueryable()
            .Where(p => p.BrandId == brandId);

        if (status is not null)
            players = players.Where(p => p.Status == (short)status);

        if (vipId is not null)
            players = players.Where(p => p.VipId == vipId);

        if (userName is not null)
            players = players.Where(p => EF.Functions.ILike(p.Username, $"{userName}%"));

        if (agentId is not null)
            players = players.Where(p => p.AgentId == agentId);

        if (phone is not null)
            players = players.Where(p => p.Phone == phone);

        if (realName is not null)
            players = players.Where(p => EF.Functions.ILike(p.RealName!, $"{realName}%"));

        if (beginRegisterTime is not null)
            players = players.Where(p => p.RegisteredOn >= beginRegisterTime.Value.ToUniversalTime());

        if (endRegisterTime is not null)
            players = players.Where(p => p.RegisteredOn < endRegisterTime.Value.ToUniversalTime());

        return players;
    }
}
