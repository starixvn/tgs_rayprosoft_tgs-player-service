using System;
using System.Collections.Generic;
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
            Status = Client.PlayerStatus.Enabled,
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
            IsSetPayPassword = false,
            VipLevelId = null,
            Status = Client.PlayerStatus.Enabled,
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

        var actual = await sut.GetPlayerInfoAsync(brandId, mail)
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
            Status = Client.PlayerStatus.Enabled,
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

        var actual = await sut.GetPlayerInfoAsync(brandId, areaCode, phone)
            .ConfigureAwait(false);

        Assert.AreEqual(expected, actual);
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

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties);

        var actual = await dbContext.PlayerProperties
            .Where(x => x.PlayerCode == playerCode)
            .ToDictionaryAsync(x => x.Name, x => x.Value);

        Assert.AreEqual(2, dbContext.PlayerProperties.Count());
        Assert.AreEqual("NewLineId", actual["Line"]);
        Assert.AreEqual("TelegramId", actual["Telegram"]);
    }
}
