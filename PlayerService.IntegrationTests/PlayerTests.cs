using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.CashService;
using Tgs.CashService.Client;
using Tgs.MarketingReport.Client;
using Tgs.PlayerService.Client;
using Tgs.PlayerService.Repositories.Entities;
using Tgs.SerialNumberGenerator;

namespace Tgs.PlayerService.IntegrationTests;

[TestFixture]
public class PlayerTests
{
    [Test]
    public async Task Create_Player()
    {
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        await using var web = new TestPlayerServiceApplication(hostBuilder => hostBuilder
            .ConfigureServices(services => services
                .AddSingleton(fakeCashServiceClient)));

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

        var walletId = Guid.NewGuid();

        _ = fakeCashServiceClient.CreateNewCashAsync(
            Arg.Is(brandId),
            Arg.Any<string>(),
            Arg.Is(currencyCode),
            Arg.Is(CashType.Player),
            Arg.Any<CancellationToken>())
            .Returns(walletId);

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
            Client.RegisterSiteType.Agent,
            agentName));

        var player = dbContext.Players.First(x => x.Username == username);
        var tag = dbContext.PlayerTags.First(x => x.PlayerCode == playerCode);

        ClassicAssert.IsNotNull(player);
        ClassicAssert.IsNotNull(player.AgentId == agentId);
        ClassicAssert.IsNotNull(playerCode);
        ClassicAssert.AreEqual(player.PlayerCode, playerCode);
        ClassicAssert.AreEqual(walletId, player.DefaultWalletId);
        ClassicAssert.IsNotNull(tag);
        _ = fakeCashServiceClient.Received(1)
            .CreateNewCashAsync(
                Arg.Is(brandId),
                Arg.Is(playerCode),
                Arg.Is(currencyCode),
                Arg.Is(CashType.Player),
                Arg.Any<CancellationToken>());
    }

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
            Password = [],
            Salt = [],
            Username = "player",
            RealName = "player1",
            DefaultWalletId = walletId,
            WithdrawPassword = [],
            WithdrawSalt = []
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
            Password = [],
            Salt = [],
            Username = "player",
            RealName = "player1",
            AccumulatedDeposit = 1000M,
            WithdrawPassword = [],
            WithdrawSalt = []
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerDepositedAmountAsync("player1");

        ClassicAssert.AreEqual(actual, 1000M);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            AgentId = Guid.NewGuid(),
            WithdrawPassword = [],
            WithdrawSalt = []
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerPropertiesAsync(playerCode);

        ClassicAssert.AreEqual(playerRow.AgentId.ToString(), actual["AgentId"]);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            RegisteredOn = expected.RegisteredOn
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerInfoAsync(playerCode)
            .ConfigureAwait(false);

        ClassicAssert.AreEqual(expected, actual);
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

        ClassicAssert.IsNull(actual);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Status = (short)PlayerStatus.RegisterUndone
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerStatusAsync(playerCode);

        ClassicAssert.AreEqual(Client.PlayerStatus.RegisterUndone, actual);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Email = mail,
            RegisteredOn = expected.RegisteredOn
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerInfoByEmailAsync(brandId, mail)
            .ConfigureAwait(false);

        ClassicAssert.AreEqual(expected, actual);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Phone = phone,
            AreaCode = areaCode,
            RegisteredOn = expected.RegisteredOn
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerInfoByPhoneAsync(brandId, areaCode, phone)
            .ConfigureAwait(false);

        ClassicAssert.AreEqual(expected, actual);
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
            Password = [],
            Salt = [],
            Username = userName,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Phone = phone,
            AreaCode = areaCode,
            RegisteredOn = expected.RegisteredOn
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerInfoByUserNameAsync(brandId, userName)
            .ConfigureAwait(false);

        ClassicAssert.AreEqual(expected, actual);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = []
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

        ClassicAssert.AreEqual("123", playerRow.AreaCode);
        ClassicAssert.AreEqual("12345678", playerRow.Phone);
        ClassicAssert.IsTrue(playerRow.PhoneAuthentication);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = []
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

        ClassicAssert.AreEqual("tester@test.com", playerRow.Email);
    }

    [Test]
    public async Task Set_Player_Id_Number()
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            IdNumber = "oldIdNumber"
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.IdNumber] = "newIdNumber"
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        ClassicAssert.AreEqual("newIdNumber", playerRow.IdNumber);
    }

    [Test]
    public async Task Set_Player_Identity_Authentication()
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            IdentityAuthentication = false
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.IdentityAuthentication] = "true"
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        ClassicAssert.True(playerRow.IdentityAuthentication);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = []
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

        ClassicAssert.AreEqual(2, dbContext.PlayerProperties.Count());
        ClassicAssert.AreEqual("NewLineId", actual["Line"]);
        ClassicAssert.AreEqual("TelegramId", actual["Telegram"]);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = []
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

        ClassicAssert.AreEqual("RealName", playerRow.RealName);
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
            Password = [],
            Salt = [],
            Username = "player1",
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Status = (short)PlayerStatus.RegisterUndone
        };

        _ = await dbContext.Players
            .AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        await sut.SetPlayerStatusAsync(playerCode, Client.PlayerStatus.Enabled);

        ClassicAssert.AreEqual((short)Client.PlayerStatus.Enabled, playerRow.Status);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = []
        });

        _ = await dbContext.SaveChangesAsync();

        var a = await sut.GetPlayerPropertiesAsync(playerCode);

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.Line] = "NewLineId"
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await sut.GetPlayerPropertiesAsync(playerCode);

        ClassicAssert.AreEqual(3, actual.Count);
        ClassicAssert.AreEqual("NewLineId", actual[nameof(Client.PlayerPropertyName.Line)]);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
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

        ClassicAssert.IsNull(playerRow.AreaCode);
        ClassicAssert.IsNull(playerRow.Phone);
        ClassicAssert.IsFalse(playerRow.PhoneAuthentication);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
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

        ClassicAssert.IsNull(playerRow.Email);
    }

    [Test]
    public async Task Unset_Player_Id_Number()
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            IdNumber = "oldIdNumber"
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.IdNumber] = null
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        ClassicAssert.IsNull(playerRow.IdNumber);
    }

    [Test]
    public async Task Unset_Player_Identity_Authentication()
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            IdentityAuthentication = true
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.IdentityAuthentication] = null
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        ClassicAssert.False(playerRow.IdentityAuthentication);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
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

        ClassicAssert.IsNull(playerRow.RealName);
    }

    [Test]
    public async Task 不能清除玩家首存時間()
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            FirstDepositOn = DateTime.UtcNow
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var depositOn = DateTime.Now;
        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.FirstDepositOn] = null
        };

        _ = Assert.ThrowsAsync<HttpRequestException>(
            async () => await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary()));

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        ClassicAssert.NotNull(playerRow.FirstDepositOn);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
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

        ClassicAssert.IsNull(playerRow.Remark);
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
                Password = [],
                Salt = [],
                Username = playerCode,
                RealName = "player1",
                WithdrawPassword = [],
                WithdrawSalt = [],
                RegisteredOn = expected.RegisteredOn
            },
            new Player
            {
                PlayerCode = parentCode,
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = [],
                Salt = [],
                Username = "parent1",
                RealName = "parent1",
                WithdrawPassword = [],
                WithdrawSalt = [],
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

        ClassicAssert.AreEqual(expected, actual);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Email = "tester@test.com",
            Remark = "test"
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerPropertiesAsync(playerCode);

        ClassicAssert.AreEqual("test", actual.GetValueOrDefault("Remark"));
    }

    [Test]
    public async Task 指定日期玩家註冊人數_查詢前天()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var reportDate = DateOnly.Parse("2023-09-10");

        var fakeSystemClock = web.Services.GetRequiredService<ISystemClock>();

        _ = fakeSystemClock.UtcNow
            .Returns(new DateTimeOffset(2023, 9, 12, 0, 0, 0, TimeSpan.FromHours(8d)));

        var fakeMarketingReportClient = web.Services.GetRequiredService<IMarketingReportClient>();
        _ = fakeMarketingReportClient
            .QueryBrandMarketingStatisticsAsync(brandId, reportDate, Arg.Any<CancellationToken>())
            .Returns(new DailyBrandMarketingStatisticInfo
            {
                BrandId = brandId,
                ReportDate = reportDate,
                DirectlyRegisterCount = 0,
                DirectlyGamingCount = 0,
                TotalGamingCount = 0,
                TotalRegisterCount = 4
            });

        var actual = await sut.CountReportDateRegisterPlayersAsync(brandId, reportDate);

        ClassicAssert.AreEqual(4, actual);
    }

    [Test]
    public async Task 指定日期玩家註冊人數_查詢昨天()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var reportDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var fakeSystemClock = web.Services.GetRequiredService<ISystemClock>();

        var now = DateTimeOffset.UtcNow;
        _ = fakeSystemClock.UtcNow
            .Returns(new DateTimeOffset(now.Year, now.Month, now.Day, 6, 0, 0, TimeSpan.FromHours(8d)));

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = "playerCode",
            BrandId = brandId,
            RegisteredOn = now.AddDays(-1).DateTime,
            DefaultWalletCurrencyCode = "CNY",
            Password = [],
            Salt = [],
            Username = "playerCode",
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Status = (short)PlayerStatus.RegisterUndone,
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.CountReportDateRegisterPlayersAsync(brandId, reportDate);

        ClassicAssert.AreEqual(1, actual);
    }

    [Test]
    public async Task 指定日期玩家註冊人數_查詢當天()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var reportDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var fakeSystemClock = web.Services.GetRequiredService<ISystemClock>();

        _ = fakeSystemClock.UtcNow
            .Returns(new DateTimeOffset(now.Year, now.Month, now.Day, 10, 0, 0, TimeSpan.FromHours(8d)));

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = "playerCode",
            BrandId = brandId,
            RegisteredOn = now.DateTime,
            DefaultWalletCurrencyCode = "CNY",
            Password = [],
            Salt = [],
            Username = "playerCode",
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Status = (short)PlayerStatus.RegisterUndone,
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.CountReportDateRegisterPlayersAsync(brandId, reportDate);

        ClassicAssert.AreEqual(1, actual);
    }

    [Test]
    public async Task 查詢玩家操作紀錄細項()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = "player1";
        var eventId = Guid.NewGuid();

        var playerEvent = new PlayerEventSource
        {
            Id = eventId,
            PlayerCode = playerCode,
            BrandId = Guid.NewGuid(),
            ChangeSets = "{}",
            Action = EventAction.Create.ToString(),
            CreatedOn = DateTime.UtcNow.AddDays(-1),
        };

        _ = await dbContext.PlayerEventSources
            .AddAsync(playerEvent);

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerHistoryAsync(eventId);

        ClassicAssert.NotNull(actual);
        ClassicAssert.AreEqual(Client.EventAction.Create, actual!.Action);
    }

    [Test]
    public async Task 查詢玩家歷史紀錄()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = "player1";
        var eventId = Guid.NewGuid();

        var playerEvent = new PlayerEventSource
        {
            Id = eventId,
            PlayerCode = playerCode,
            BrandId = Guid.NewGuid(),
            ChangeSets = "{}",
            Action = EventAction.Create.ToString(),
            CreatedOn = DateTime.UtcNow.AddDays(-1),
        };

        _ = await dbContext.PlayerEventSources
            .AddAsync(playerEvent);

        _ = await dbContext.SaveChangesAsync();

        var conditions = new GetPlayerHistoriesQueryConditions(playerCode, DateTime.Now.AddDays(-14), DateTime.Now);

        var actual = await sut.GetPlayerHistoriesAsync(conditions, 10, 0).ToArrayAsync();

        ClassicAssert.AreEqual(1, actual.Length);
        ClassicAssert.AreEqual(Client.EventAction.Create, actual[0].Action);
        ClassicAssert.AreEqual(eventId, actual[0].EventId);
    }

    [Test]
    public async Task 查詢玩家歷史紀錄筆數()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = "player1";
        var eventId = Guid.NewGuid();

        var playerEvent = new PlayerEventSource
        {
            Id = eventId,
            PlayerCode = playerCode,
            BrandId = Guid.NewGuid(),
            ChangeSets = "{}",
            Action = EventAction.Create.ToString(),
            CreatedOn = DateTime.UtcNow.AddDays(-1),
        };

        _ = await dbContext.PlayerEventSources
            .AddAsync(playerEvent);

        _ = await dbContext.SaveChangesAsync();

        var conditions = new GetPlayerHistoriesQueryConditions(playerCode, DateTime.Now.AddDays(-14), DateTime.Now);

        var actual = await sut.CountPlayerHistoriesAsync(conditions);

        ClassicAssert.AreEqual(1, actual);
    }

    [Test]
    public async Task 查詢當日玩家註冊人數()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = "playerCode",
            BrandId = brandId,
            RegisteredOn = DateTime.UtcNow,
            DefaultWalletCurrencyCode = "CNY",
            Password = [],
            Salt = [],
            Username = "playerCode",
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Status = (short)PlayerStatus.RegisterUndone,
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.CountCurrentDayRegisterPlayersAsync(brandId);

        ClassicAssert.AreEqual(1, actual);
    }

    [Test]
    public async Task 設定玩家狀態為註冊未完成()
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
            Password = [],
            Salt = [],
            Username = "player1",
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Status = (short)PlayerStatus.RegisterUndone
        };

        _ = await dbContext.Players
            .AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        await sut.SetPlayerStatusAsync(playerCode, Client.PlayerStatus.RegisterUndone, null);

        ClassicAssert.AreEqual((short)Client.PlayerStatus.RegisterUndone, playerRow.Status);
    }

    [Test]
    public async Task 設定玩家首存時間()
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = []
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        var depositOn = DateTime.Now;
        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.FirstDepositOn] = depositOn.ToString("o")
        };

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties.ToImmutableDictionary());

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        ClassicAssert.AreEqual(depositOn.ToUniversalTime(), playerRow.FirstDepositOn);
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
            Password = [],
            Salt = [],
            Username = playerCode,
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = []
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

        ClassicAssert.AreEqual("test", playerRow.Remark);
    }

    [Test]
    public async Task 變更玩家密碼()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var playerCode = "4ucmqplxb029";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            BrandId = brandId,
            DefaultWalletCurrencyCode = "CNY",
            Password = [],
            Salt = [],
            Username = playerCode,
            WithdrawPassword = [],
            WithdrawSalt = []
        };

        _ = await dbContext.Players.AddAsync(playerRow);
        _ = await dbContext.SaveChangesAsync();

        var password = "p12345678";
        var modifyUsername = "admin.enabled";
        var modifyUserId = "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5";

        await sut.SetPlayerPasswordAsync(playerCode, password, modifyUsername, modifyUserId);

        var actual = await dbContext.Players.FirstAsync(p => p.PlayerCode == playerCode);

        ClassicAssert.AreNotEqual(Array.Empty<byte>(), actual.Password);
        ClassicAssert.AreNotEqual(Array.Empty<byte>(), actual.Salt);
        ClassicAssert.AreEqual(modifyUsername, actual.UpdatedByUsername);

        var playerEvent = await dbContext.PlayerEventSources.FirstOrDefaultAsync(p => p.PlayerCode == playerCode);
        ClassicAssert.NotNull(playerEvent);

        ClassicAssert.AreEqual(playerEvent!.Id, actual.SnapshotEventId);
    }

    [Test]
    public async Task Query_Player_Birthday()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var birthday = DateOnly.Parse("2024-02-05");
        var page = 1;
        var rows = 2;

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = "test",
            BrandId = Guid.NewGuid(),
            DefaultWalletCurrencyCode = "CNY",
            Password = [],
            Salt = [],
            Username = "test",
            RealName = "player1",
            WithdrawPassword = [],
            WithdrawSalt = [],
            Status = (short)PlayerStatus.Enabled,
            Birthdate = DateOnly.Parse("2024-02-05"),
            Birthday = "0205"
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.QueryPlayerBirthdayAsync(birthday, page, rows).ToArrayAsync();

        ClassicAssert.AreEqual("test", actual.FirstOrDefault());
    }
}
