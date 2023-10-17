using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tgs.Core.Infrastructures.Utility;
using Tgs.Paging;
using Tgs.PlayerService.Repositories.Entities;
using Tgs.SerialNumberGenerator;

namespace Tgs.PlayerService.Repositories;

internal class PlayerRepository : IPlayerRepository
{
    private readonly TgsContext _dbContext;
    private readonly ISerialNumberGenerator _serialNumberGenerator;

    public PlayerRepository(TgsContext context, ISerialNumberGenerator serialNumberGenerator)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        _serialNumberGenerator =
            serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
    }

    public async ValueTask AddPlayerAuditLogAsync(
        Guid createId,
        string userName,
        string moduleName,
        string operation,
        string modifier,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new PlayerAuditLog
        {
            Id = createId,
            ModuleName = "Player",
            CreatedByUsername = modifier,
            CreatedOn = DateTime.UtcNow,
            Operation = operation,
            Username = userName
        };

        _ = await _dbContext.PlayerAuditLogs
            .AddAsync(auditLog, cancellationToken)
            .ConfigureAwait(false);

        _ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<bool> CheckSameEmailByBrandAsync(Guid brandId, string playerCode, string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Players.AnyAsync(p
            => p.BrandId == brandId
            && p.PlayerCode != playerCode
            && p.Email!.ToLower() == email.ToLower(), cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<int> CountCurrentDayLoginAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        var currentDay = new DateTimeOffset(
            DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date,
            TimeSpan.FromHours(8D))
            .UtcDateTime;

        var query = from player in _dbContext.Players.AsQueryable()
                    where player.BrandId == brandId
                    join ipRecord in _dbContext.PlayerIpRecords
                        on new
                        {
                            player.PlayerCode,
                            IpType = (short)PlayerIpType.Login
                        } equals new
                        {
                            ipRecord.PlayerCode,
                            ipRecord.IpType
                        } into pirg
                    from ipRecord in pirg.DefaultIfEmpty()
                    where ipRecord.RecordOn >= currentDay
                    select new
                    {
                        player.PlayerCode
                    };

        return await query
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<int> CountPlayerEventAsync(PlayerEventQueryConditions conditions, CancellationToken cancellationToken = default)
        => await MakePlayerHistoriesQuery(conditions)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

    public async ValueTask<int> CountYesterdayReigsterAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        var currentDay = new DateTimeOffset(
            DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date,
            TimeSpan.FromHours(8D))
            .UtcDateTime;

        var yesterday = currentDay.AddDays(-1);

        return await _dbContext.Players
            .Where(p => p.BrandId == brandId
                && p.RegisteredOn >= yesterday
                && p.RegisteredOn < currentDay)
            .CountAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<string> CreatePlayerAsync(CreatePlayerData data,
            CancellationToken cancellationToken = default)
    {
        var username = data.UserName;
        var walletId = await _serialNumberGenerator.GenerateAsync(cancellationToken);
        var createdOn = DateTime.UtcNow;
        var playerCode = PlayerCodeGenerator.NewId();

        var player = new Player
        {
            PlayerCode = playerCode,
            Username = username,
            Password = data.Password,
            Salt = data.Salt,
            WithdrawPassword = data.WithdrawPassword,
            WithdrawSalt = data.WithdrawSalt,
            BrandId = data.BrandId,
            DefaultWalletId = walletId,
            DefaultWalletCurrencyCode = data.CurrencyCode,
            LastActiveOn = createdOn,
            Status = (short)data.PlayerStatus,
            Type = (short)data.Type,
            BetLimitId = data.BetLimitId,
            AgentId = data.AgentId,
            UpdatedByUsername = username,
            UpdatedOn = createdOn,
            VipId = data.VipId
        };

        var wallet = new Wallet
        {
            Id = walletId,
            PlayerCode = playerCode,
            CurrencyCode = data.CurrencyCode,
            ExternalCurrencyCode = data.CurrencyCode,
            Balance = 0,
            CreatedOn = createdOn,
            UpdatedOn = createdOn
        };

        _ = await _dbContext.Players.AddAsync(player, cancellationToken);
        _ = await _dbContext.Wallets.AddAsync(wallet, cancellationToken);

        if (!string.IsNullOrWhiteSpace(data.UpperPlayerCode))
            _ = await _dbContext.PlayerParents.AddAsync(new PlayerParent
            {
                PlayerCode = playerCode,
                ParentPlayerCode = data.UpperPlayerCode
            }, cancellationToken).ConfigureAwait(false);

        _ = await _dbContext.SaveChangesAsync(cancellationToken);

        return playerCode;
    }

    public async ValueTask CreatePlayerEventAsync(
        Guid eventId,
        Guid brandId,
        string playerCode,
        EventAction action,
        JsonDocument changeSets,
        string? modifier,
        CancellationToken cancellationToken = default)
    {
        _ = await _dbContext.PlayerEventSources.AddAsync(
            new PlayerEventSource
            {
                Id = eventId,
                BrandId = brandId,
                Action = action.ToString(),
                PlayerCode = playerCode,
                ChangeSets = JsonSerializer.Serialize(changeSets),
                ModifyUser = modifier,
                CreatedOn = DateTime.UtcNow,
            },
            cancellationToken).ConfigureAwait(false);

        _ = await _dbContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask CreatePlayerTagAsync(string playerCode, string tag,
        CancellationToken cancellationToken = default)
    {
        var playerTag = new PlayerTag
        {
            PlayerCode = playerCode,
            Tag = tag
        };

        _ = await _dbContext.PlayerTags.AddAsync(playerTag, cancellationToken);

        _ = await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask<int> CurrentDayRegisterAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        var currentDay = new DateTimeOffset(
            DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date,
            TimeSpan.FromHours(8D))
            .UtcDateTime;

        return await _dbContext.Players
            .Where(p => p.BrandId == brandId && p.RegisteredOn >= currentDay)
            .CountAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<string?> GetParentPlayerCodeAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
        => await _dbContext.PlayerParents
            .Where(pp => pp.PlayerCode == playerCode)
            .Select(pp => pp.ParentPlayerCode)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

    public IAsyncEnumerable<PlayerCountInfo> GetPlayerCountByBrandIdAsync(Guid brandId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Players.Where(x => x.BrandId == brandId)
            .GroupBy(x => x.AgentId, (agentId, list) => new PlayerCountInfo(agentId, list.Count())).ToAsyncEnumerable();
    }

    public async ValueTask<decimal> GetPlayerDepositedAmountAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[]
            {
                playerCode
            }, cancellationToken)
            .ConfigureAwait(false);

        return player?.AccumulatedDeposit ?? 0;
    }

    public async ValueTask<EventInfo?> GetPlayerEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var eventSource = await _dbContext.PlayerEventSources
            .FirstOrDefaultAsync(p => p.Id == eventId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return eventSource is null
            ? null
            : new EventInfo(
                Enum.Parse<EventAction>(eventSource.Action),
                JsonDocument.Parse(eventSource.ChangeSets),
                eventSource.ModifyUser,
                eventSource.CreatedOn);
    }

    public async ValueTask<PlayerInfo?> GetPlayerInfoAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[]
            {
                playerCode
            }, cancellationToken)
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

    public async ValueTask<PlayerInfo?> GetPlayerInfoByEmailAsync(
        Guid brandId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players.AsQueryable()
            .Where(player => player.BrandId == brandId
                && player.Email!.ToLower() == email.ToLower())
            .FirstOrDefaultAsync(cancellationToken)
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

    public async ValueTask<PlayerInfo?> GetPlayerInfoByPhoneAsync(Guid brandId, string areaCode, string phone,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players.AsQueryable()
            .Where(player => player.BrandId == brandId
                && player.AreaCode == areaCode
                && player.Phone == phone)
            .FirstOrDefaultAsync(cancellationToken)
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

    public async ValueTask<PlayerInfo?> GetPlayerInfoByUserNameAsync(Guid brandId, string userName,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players.AsQueryable()
            .Where(player => player.BrandId == brandId
                && player.Username!.ToLower() == userName.ToLower())
            .FirstOrDefaultAsync(cancellationToken)
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

    public IAsyncEnumerable<PlayerInfo> GetPlayerInfosByUserNamesAsync(
        Guid brandId,
        IEnumerable<string> userNames,
        CancellationToken cancellationToken = default)
        => _dbContext.Players.AsQueryable()
            .Where(p => userNames.Contains(p.Username))
            .Select(p => new
            {
                p.PlayerCode,
                p.BrandId,
                p.Username,
                p.IdentityAuthentication,
                p.PhoneAuthentication,
                p.Email,
                p.WithdrawPassword,
                p.VipId,
                p.RegisteredOn
            })
            .ToAsyncEnumerable()
            .Select(p => new PlayerInfo(
                p.PlayerCode,
                p.BrandId,
                p.Username,
                p.IdentityAuthentication,
                p.PhoneAuthentication,
                !string.IsNullOrEmpty(p.Email),
                p.WithdrawPassword.Length > 0,
                p.VipId,
                p.RegisteredOn));

    public async ValueTask<PlayerLoginInfo?> GetPlayerLoginInfoAsync(string playerCode,
        CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.PlayerIpRecords
            .Where(record => record.PlayerCode == playerCode)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var registerRecord = records
            .Where(record => record.IpType == 1)
            .FirstOrDefault();

        var loginRecord = records
            .Where(record => record.IpType == 2)
            .FirstOrDefault();

        return records.Length == 0
            ? null
            : new PlayerLoginInfo(
                registerRecord?.Ip,
                registerRecord?.Host,
                loginRecord?.Ip,
                loginRecord?.RecordOn);
    }

    public async ValueTask<IImmutableDictionary<string, string>> GetPlayerPropertiesAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[]
            {
                playerCode
            }, cancellationToken)
            .ConfigureAwait(false);

        var properties = new Dictionary<string, string>();

        if (player is null)
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
        if (player.Birthdate is not null)
            properties["Birthdate"] = player.Birthdate.ToString()!;
        if (!string.IsNullOrEmpty(player.Remark))
            properties["Remark"] = player.Remark;

        properties["DefaultWalletId"] = player.DefaultWalletId.ToString();
        if (player.AgentId is not null)
            properties["AgentId"] = player.AgentId.Value.ToString();

        foreach (var prop in await _dbContext.PlayerProperties.AsQueryable()
            .Where(pp => pp.BrandId == brandId && pp.PlayerCode == playerCode)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false))
            properties[prop.Name] = prop.Value;

        return properties.ToImmutableDictionary();
    }

    public async ValueTask<PlayerStatus> GetPlayerStatusAsync(string playerCode,
        CancellationToken cancellationToken = default)
        => (PlayerStatus)(await _dbContext.Players
            .Where(player => player.PlayerCode == playerCode)
            .Select(player => (short?)player.Status)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? 1);

    public IAsyncEnumerable<PlayerHistoryInfo> QueryPlayerEventAsync(
        PlayerEventQueryConditions conditions,
        int skip = 0,
        CancellationToken cancellationToken = default)
    {
        var dataQuery = MakePlayerHistoriesQuery(conditions);

        return dataQuery
            .OrderByDescending(record => record.CreatedOn)
            .Select(record => new
            {
                record.Id,
                record.Action,
                record.ModifyUser,
                record.CreatedOn,
            })
            .Skip(skip)
            .Take(1000)
            .ToAsyncEnumerable()
            .Select(row => new PlayerHistoryInfo(
                row.Id,
                Enum.Parse<EventAction>(row.Action),
                row.ModifyUser,
                row.CreatedOn));
    }

    public async IAsyncEnumerable<PlayerInfo> QueryPlayerInfoListAsync(
        Guid brandId,
        IEnumerable<string> playerCodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var players = await _dbContext.Players
            .Where(p => p.BrandId == brandId && playerCodes.Contains(p.PlayerCode))
            .Select(p => new
            {
                p.PlayerCode,
                p.BrandId,
                p.Username,
                p.IdentityAuthentication,
                p.PhoneAuthentication,
                p.Email,
                p.WithdrawPassword,
                p.VipId,
                p.RegisteredOn
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var player in players)
            yield return new PlayerInfo(
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

    public IAsyncEnumerable<PlayerInfo> QueryPlayerInfosByUserNameAsync(
        Guid brandId,
        string userNamePrefix,
        CancellationToken cancellationToken = default)
        => InternalQueryPlayerList(
            brandId,
            null,
            null,
            userNamePrefix,
            null,
            null,
            null,
            null,
            null)
            .Select(p => new
            {
                p.PlayerCode,
                p.BrandId,
                p.Username,
                p.IdentityAuthentication,
                p.PhoneAuthentication,
                p.Email,
                p.WithdrawPassword,
                p.VipId,
                p.RegisteredOn
            })
            .OrderBy(p => p.Username)
            .Take(1000)
            .ToAsyncEnumerable()
            .Select(p => new PlayerInfo(
                p.PlayerCode,
                p.BrandId,
                p.Username,
                p.IdentityAuthentication,
                p.PhoneAuthentication,
                !string.IsNullOrEmpty(p.Email),
                p.WithdrawPassword.Length > 0,
                p.VipId,
                p.RegisteredOn));

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

        var query = from player in players
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
                        on new
                        {
                            player.PlayerCode,
                            IpType = (short)PlayerIpType.Register
                        } equals new
                        {
                            ipRecord.PlayerCode,
                            ipRecord.IpType
                        } into pirg
                    from ipRecord in pirg.DefaultIfEmpty()
                    select new
                    {
                        player.PlayerCode,
                        player.Username,
                        player.RealName,
                        player.RegisteredOn,
                        ipRecord.Host,
                        player.AgentId,
                        player.VipId,
                        player.Status,
                        parent.ParentPlayerCode
                    };

        if (registeredDomainName is not null)
            query = query.Where(item => item.Host.ToLower() == registeredDomainName.ToLower());

        if (parentPlayerCode is not null)
            query = query.Where(item => item.ParentPlayerCode == parentPlayerCode);

        return query
            .OrderByDescending(item => item.RegisteredOn != null)
            .ThenByDescending(item => item.RegisteredOn)
            .Take(1000)
            .ToArray()
            .Select(item => new PlayerListItem(
                item.PlayerCode,
                item.Username,
                item.RealName,
                item.RegisteredOn,
                item.Host,
                item.AgentId,
                item.VipId,
                (PlayerStatus)item.Status,
                item.ParentPlayerCode))
            .ToAsyncEnumerable();
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

        var query = from player in players
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
                        on new
                        {
                            player.PlayerCode,
                            IpType = (short)PlayerIpType.Register
                        } equals new
                        {
                            ipRecord.PlayerCode,
                            ipRecord.IpType
                        } into pirg
                    from ipRecord in pirg.DefaultIfEmpty()
                    select new
                    {
                        player.PlayerCode,
                        player.Username,
                        player.RealName,
                        player.RegisteredOn,
                        ipRecord.Host,
                        player.AgentId,
                        player.VipId,
                        player.Status,
                        parent.ParentPlayerCode
                    };

        if (registeredDomainName is not null)
            query = query.Where(item => item.Host.ToLower() == registeredDomainName.ToLower());

        if (parentPlayerCode is not null)
            query = query.Where(item => item.ParentPlayerCode == parentPlayerCode);

        return query
            .OrderByDescending(item => item.RegisteredOn != null)
            .ThenByDescending(item => item.RegisteredOn)
            .Take(1000)
            .ToArray()
            .Select(item => new PlayerListItem(
                item.PlayerCode,
                item.Username,
                item.RealName,
                item.RegisteredOn,
                item.Host,
                item.AgentId,
                item.VipId,
                (PlayerStatus)item.Status,
                item.ParentPlayerCode))
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<PlayerListItem> QueryPlayerListWithoutLimitAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        var players = InternalQueryPlayerList(
            brandId,
            PlayerStatus.Enabled,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        var query = from player in players
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
                        on new
                        {
                            player.PlayerCode,
                            IpType = (short)PlayerIpType.Register
                        } equals new
                        {
                            ipRecord.PlayerCode,
                            ipRecord.IpType
                        } into pirg
                    from ipRecord in pirg.DefaultIfEmpty()
                    select new
                    {
                        player.PlayerCode,
                        player.Username,
                        player.RealName,
                        player.RegisteredOn,
                        ipRecord.Host,
                        player.AgentId,
                        player.VipId,
                        player.Status,
                        parent.ParentPlayerCode
                    };

        return query
            .Select(item => new PlayerListItem(
                item.PlayerCode,
                item.Username,
                item.RealName,
                item.RegisteredOn,
                item.Host,
                item.AgentId,
                item.VipId,
                (PlayerStatus)item.Status,
                item.ParentPlayerCode))
            .ToAsyncEnumerable();
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
                    RecordOn = recordTimeUtc
                }, cancellationToken)
                .ConfigureAwait(false)).Entity;

        record.Ip = ip;
        record.Host = host;
        record.RecordOn = recordTimeUtc;

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
            .FindAsync(new object?[]
            {
                playerCode
            }, cancellationToken)
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

    public async ValueTask SetPlayerPasswordAsync(
        string playerCode,
        byte[] password,
        byte[] passwordSalt,
        string? modifyUsername,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[]
            {
                playerCode
            }, cancellationToken)
            .ConfigureAwait(false);

        if (player == null)
            return;

        player.Password = password;
        player.Salt = passwordSalt;

        player.UpdatedByUsername = modifyUsername;
        player.UpdatedOn = DateTime.UtcNow;

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
            .FindAsync(new object?[]
            {
                playerCode
            }, cancellationToken)
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

    public async ValueTask SetPlayerStatusAsync(string playerCode, PlayerStatus status,
        CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players
            .FindAsync(new object?[]
            {
                playerCode
            }, cancellationToken)
            .ConfigureAwait(false);

        if (player == null)
            return;

        player.Status = (short)status;
        player.UpdatedOn = DateTime.UtcNow;

        _ = await _dbContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask TopUpAccumulatedDepositAsync(string playerCode, decimal amount, CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.Players.FindAsync(
            new object?[] { playerCode },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (player is null)
            return;

        player.AccumulatedDeposit += amount;

        player.UpdatedByUsername = playerCode;
        player.UpdatedOn = DateTime.UtcNow;

        _ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask UpdatePlayerSnapshotAsync(string playerCode, Guid snapshotId, DateTime updatedOn, CancellationToken cancellationToken = default)
    {
        var dbSnapshot = await _dbContext.Players
            .FindAsync(new object?[] { playerCode }, cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? (await _dbContext.Players.AddAsync(new Player
            {
                PlayerCode = playerCode
            }, cancellationToken).ConfigureAwait(false)).Entity;

        if (dbSnapshot.SnapshotEventId is null || dbSnapshot.SnapshotEventId.Value.CompareTo(snapshotId) == -1)
        {
            dbSnapshot.SnapshotEventId = snapshotId;
            dbSnapshot.UpdatedOn = updatedOn;
        }

        _ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
            players = players.Where(p => p.Username.ToLower().StartsWith(userName.ToLower()));

        if (agentId is not null)
            players = players.Where(p => p.AgentId == agentId);

        if (phone is not null)
            players = players.Where(p => p.Phone == phone);

        if (realName is not null)
            players = players.Where(p => p.RealName!.ToLower().StartsWith(realName.ToLower()));

        if (beginRegisterTime is not null)
            players = players.Where(p => p.RegisteredOn >= beginRegisterTime.Value.ToUniversalTime());

        if (endRegisterTime is not null)
            players = players.Where(p => p.RegisteredOn < endRegisterTime.Value.ToUniversalTime());

        return players;
    }

    private IQueryable<PlayerEventSource> MakePlayerHistoriesQuery(PlayerEventQueryConditions conditions)
    {
        var dataQuery = from p in _dbContext.PlayerEventSources.AsQueryable()
                        where p.PlayerCode == conditions.PlayerCode
                            && p.CreatedOn >= conditions.From
                            && p.CreatedOn < conditions.To
                        select p;

        return dataQuery;
    }
}
