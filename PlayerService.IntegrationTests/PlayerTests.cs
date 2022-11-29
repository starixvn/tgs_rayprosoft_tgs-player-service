using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tgs.PlayerService.Client;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.IntegrationTests;

[TestFixture]
public class PlayerTests
{
    [Test]
    public async Task Get_DefaultWalletId()
    {
        var walletId = Guid.NewGuid();

        var playerCode = "player1";

        var playerProperties = new Dictionary<string, string>
        {
            ["RealName"] = "player1",
            ["DefaultWalletId"] = walletId.ToString()
        };

        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = "player",
            RealName = "player1",
            DefaultWalletId = walletId,
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerPropertiesAsync("player1");

        CollectionAssert.AreEqual(walletId.ToString(), actual["DefaultWalletId"]);
    }

    [Test]
    public async Task Get_Deposited_Amount()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = "player1",
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = "player",
            RealName = "player1",
            AccumulatedDeposit = 1000M,
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerDepositedAmountAsync("player1");

        Assert.AreEqual(actual, 1000M);
    }

    [Test]
    public async Task Get_Player_AgentId()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            AgentId = Guid.NewGuid(),
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerPropertiesAsync(playerCode);

        Assert.AreEqual(playerRow.AgentId.ToString(), actual["AgentId"]);
    }

    [Test]
    public async Task Get_Player_Info()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var expected = new Client.PlayerInfo
        {
            BrandId = brandId,
            PlayerCode = playerCode,
            UserName = playerCode,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            IsSetPayPassword = false,
            VipLevelId = null,
            RegisteredOn = DateTime.Parse("2022/05/17 00:00:00")
        };

        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            RegisteredOn = expected.RegisteredOn
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerInfoAsync(playerCode)
            .ConfigureAwait(false);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task Get_Player_Info_When_Player_Not_Found()
    {
        var playerCode = "player1";

        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var actual = await sut.GetPlayerInfoAsync(playerCode)
            .ConfigureAwait(false);

        Assert.IsNull(actual);
    }

    [Test]
    public async Task Get_Player_Properties()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var playerCode = "player1";
        var brandId = Guid.NewGuid();
        var defaultWalletId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = "player1",
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            DefaultWalletId = defaultWalletId
        });

        _ = await dbContext.PlayerProperties.AddAsync(new PlayerProperty
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            Name = "Viber",
            Value = "ViberId"
        });

        _ = await dbContext.SaveChangesAsync();

        var expected = new Dictionary<string, string>
        {
            ["RealName"] = "player1",
            ["Viber"] = "ViberId",
            ["DefaultWalletId"] = defaultWalletId.ToString()
        };

        var actual = await sut.GetPlayerPropertiesAsync("player1");

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public async Task Get_Player_Status()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            BrandId = Guid.NewGuid(),
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            Status = (short)PlayerStatus.RegisterUndone
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerStatusAsync(playerCode);

        Assert.AreEqual(Client.PlayerStatus.RegisterUndone, actual);
    }

    [Test]
    public async Task Get_PlayerInfo_By_Email()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";
        var mail = "tester@test.com";

        var expected = new Client.PlayerInfo
        {
            BrandId = brandId,
            PlayerCode = playerCode,
            UserName = playerCode,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            EmailAuthentication = true,
            IsSetPayPassword = false,
            VipLevelId = null,
            RegisteredOn = DateTime.Parse("2022/05/17 00:00:00")
        };

        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            Email = mail,
            RegisteredOn = expected.RegisteredOn
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerInfoByEmailAsync(brandId, mail)
            .ConfigureAwait(false);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task Get_PlayerInfo_By_Phone()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";
        var phone = "13800138000";
        var areaCode = "123";

        var expected = new Client.PlayerInfo
        {
            BrandId = brandId,
            PlayerCode = playerCode,
            UserName = playerCode,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            IsSetPayPassword = false,
            VipLevelId = null,
            RegisteredOn = DateTime.Parse("2022/05/17 00:00:00")
        };

        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            Phone = phone,
            AreaCode = areaCode,
            RegisteredOn = expected.RegisteredOn
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerInfoByPhoneAsync(brandId, areaCode, phone)
            .ConfigureAwait(false);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task Get_PlayerInfo_By_UserName()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";
        var userName = "Tester";
        var phone = "13800138000";
        var areaCode = "123";

        var expected = new Client.PlayerInfo
        {
            BrandId = brandId,
            PlayerCode = playerCode,
            UserName = userName,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            IsSetPayPassword = false,
            VipLevelId = null,
            RegisteredOn = DateTime.Parse("2022/05/17 00:00:00")
        };

        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = userName,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            Phone = phone,
            AreaCode = areaCode,
            RegisteredOn = expected.RegisteredOn
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerInfoByUserNameAsync(brandId, userName)
            .ConfigureAwait(false);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task Set_Player_AreaCode_And_Phone()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.AreaCode] = "123",
            [Client.PlayerPropertyName.Phone] = "12345678",
            [Client.PlayerPropertyName.PhoneAuthentication] = true.ToString()
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.AreEqual("123", playerRow.AreaCode);
        Assert.AreEqual("12345678", playerRow.Phone);
        Assert.IsTrue(playerRow.PhoneAuthentication);
    }

    [Test]
    public async Task Set_Player_Email()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.Email] = "tester@test.com"
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.AreEqual("tester@test.com", playerRow.Email);
    }

    [Test]
    public async Task Set_Player_Properties()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        });

        await dbContext.PlayerProperties.AddRangeAsync(
            new PlayerProperty
            {
                PlayerCode = playerCode,
                BrandId = brandId,
                Name = "Viber",
                Value = "ViberId"
            },
            new PlayerProperty
            {
                PlayerCode = playerCode,
                BrandId = brandId,
                Name = "Line",
                Value = "LineId"
            });

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.Viber] = null,
            [Client.PlayerPropertyName.Line] = "NewLineId",
            [Client.PlayerPropertyName.Telegram] = "TelegramId"
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.AreEqual(2, dbContext.PlayerProperties.Count());
        Assert.AreEqual("NewLineId", actual["Line"]);
        Assert.AreEqual("TelegramId", actual["Telegram"]);
    }

    [Test]
    public async Task Set_Player_RealName()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.RealName] = "RealName"
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.AreEqual("RealName", playerRow.RealName);
    }

    [Test]
    public async Task Set_Player_Status()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = "player1";

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = Guid.NewGuid(),
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = "player1",
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            Status = (short)PlayerStatus.RegisterUndone
        };

        _ = await dbContext.Players
            .AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        await sut.SetPlayerStatusAsync(playerCode, Client.PlayerStatus.Enabled);

        Assert.AreEqual((short)Client.PlayerStatus.Enabled, playerRow.Status);
    }

    [Test]
    public async Task Set_PlayerProperties_After_Get_By_Cache_Should_Correct()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        });

        _ = await dbContext.SaveChangesAsync();

        var a = await sut.GetPlayerPropertiesAsync(playerCode);

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.Line] = "NewLineId"
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await sut.GetPlayerPropertiesAsync(playerCode);

        Assert.AreEqual(3, actual.Count);
        Assert.AreEqual("NewLineId", actual[nameof(Client.PlayerPropertyName.Line)]);
    }

    [Test]
    public async Task Unset_Player_AreaCode_And_Phone()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            AreaCode = "123",
            Phone = "12345678",
            PhoneAuthentication = true
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.AreaCode] = null,
            [Client.PlayerPropertyName.Phone] = null,
            [Client.PlayerPropertyName.PhoneAuthentication] = null
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.IsNull(playerRow.AreaCode);
        Assert.IsNull(playerRow.Phone);
        Assert.IsFalse(playerRow.PhoneAuthentication);
    }

    [Test]
    public async Task Unset_Player_Email()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            Email = "tester@test.com"
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.Email] = null
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.IsNull(playerRow.Email);
    }

    [Test]
    public async Task Unset_Player_RealName()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            Email = "tester@test.com"
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.RealName] = null
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.IsNull(playerRow.RealName);
    }

    [Test]
    public async Task 取消設定玩家備註()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            Email = "tester@test.com",
            Remark = "test"
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.Remark] = null
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.IsNull(playerRow.Remark);
    }

    [Test]
    public async Task 取得ParentPlayerInfo()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";
        var parentCode = "parent1";

        var expected = new Client.PlayerInfo
        {
            BrandId = brandId,
            PlayerCode = parentCode,
            UserName = "parent1",
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            IsSetPayPassword = false,
            VipLevelId = null,
            RegisteredOn = DateTime.Parse("2022/05/17 00:00:00")
        };

        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = playerCode,
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = playerCode,
                RealName = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = expected.RegisteredOn
            },
            new Player
            {
                PlayerCode = parentCode,
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "parent1",
                RealName = "parent1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = expected.RegisteredOn
            });

        _ = await dbContext.PlayerParents.AddAsync(new PlayerParent
        {
            PlayerCode = playerCode,
            ParentPlayerCode = parentCode
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetParentPlayerInfoAsync(playerCode)
            .ConfigureAwait(false);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task 取得玩家備註()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            Email = "tester@test.com",
            Remark = "test"
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerPropertiesAsync(playerCode);

        Assert.AreEqual("test", actual.GetValueOrDefault("Remark"));
    }

    [Test]
    public async Task 設定玩家備註()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.Remark] = "test"
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.AreEqual("test", playerRow.Remark);
    }

    [Test]
    public async Task Create_Player()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var vipId = Guid.NewGuid();
        var betLimitId = 1;
        var currencyCode = "currencyCode";
        var username = "username";
        var password = "password";
        var agentName = "agentName";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = await sut.CreatePlayerAsync(new Client.CreatePlayerData(
            brandId,
            agentId,
            vipId,
            currencyCode,
            username,
            password,
            null,
            betLimitId,
            Client.PlayerType.RealPlayer,
            Client.RegisterPlatformType.Agent,
            agentName));

        var player = dbContext.Players.First(x => x.Username== username);

        Assert.IsNotNull(player);
        Assert.IsNotNull(player.AgentId == agentId);
        Assert.IsNotNull(playerCode);
    }
}
