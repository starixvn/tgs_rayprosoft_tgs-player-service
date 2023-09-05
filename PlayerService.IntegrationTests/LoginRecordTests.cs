using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tgs.PlayerService.Client;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.IntegrationTests;

public class LoginRecordTests
{
    [Test]
    public async Task Record_Login_Ip()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = "test1";

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = "player",
            RealName = "player1",
            DefaultWalletId = Guid.NewGuid(),
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        });

        _ = await dbContext.SaveChangesAsync();

        await sut.RecordLoginIpAsync(playerCode, "127.0.0.1", "localhost");

        var actual = await dbContext.PlayerIpRecords.ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual(2, actual.First().IpType);
    }

    [Test]
    public async Task Record_Register_Ip()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = "test1";

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = "player",
            RealName = "player1",
            DefaultWalletId = Guid.NewGuid(),
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        });

        _ = await dbContext.SaveChangesAsync();

        await sut.RecordRegisterIpAsync(playerCode, "127.0.0.1", "localhost");

        var actual = await dbContext.PlayerIpRecords.ToArrayAsync();

        Assert.AreEqual(2, actual.Length);
        Assert.IsTrue(actual.Any(r => r.IpType == 1));
        Assert.IsTrue(actual.Any(r => r.IpType == 2));
    }

    [Test]
    public async Task Record_Register_Ip_Already_Has_Login_Ip_Record()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = "test1";

        _ = await dbContext.Players.AddAsync(new Player
        {
            PlayerCode = playerCode,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = "player",
            RealName = "player1",
            DefaultWalletId = Guid.NewGuid(),
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        });

        _ = await dbContext.PlayerIpRecords.AddAsync(
            new PlayerIpRecord
            {
                PlayerCode = playerCode,
                IpType = 2,
                Ip = "127.0.0.1",
                RecordOn = DateTime.Now
            });

        _ = await dbContext.SaveChangesAsync();

        await sut.RecordRegisterIpAsync(playerCode, "127.0.0.1", "localhost");

        var actual = await dbContext.PlayerIpRecords.ToArrayAsync();

        Assert.AreEqual(2, actual.Length);
        Assert.IsTrue(actual.Any(r => r.IpType == 1));
        Assert.IsTrue(actual.Any(r => r.IpType == 2));
    }

    [Test]
    public async Task 取得玩家登入Ip及註冊Ip()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = "test1";

        await dbContext.PlayerIpRecords.AddRangeAsync(
            new PlayerIpRecord
            {
                IpType = 1,
                Host = "localhost",
                Ip = "127.0.0.1",
                PlayerCode = playerCode,
                RecordOn = DateTime.UtcNow
            },
            new PlayerIpRecord
            {
                IpType = 2,
                Host = "localhost",
                Ip = "127.0.0.2",
                PlayerCode = playerCode,
                RecordOn = DateTime.UtcNow
            });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.GetPlayerLoginInfoAsync(playerCode);

        Assert.IsNotNull(actual);
        Assert.AreEqual("127.0.0.1", actual!.RegisterIp);
        Assert.AreEqual("127.0.0.2", actual!.LoginIp);
    }

    [Test]
    public async Task 查詢當日玩家登入總人數()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var playerCode1 = "playerCode1";
        var playerCode2 = "playerCode2";

        await dbContext.PlayerIpRecords.AddRangeAsync(
            new PlayerIpRecord
            {
                IpType = 1,
                Host = "localhost",
                Ip = "127.0.0.1",
                PlayerCode = playerCode1,
                RecordOn = DateTime.UtcNow
            },
            new PlayerIpRecord
            {
                IpType = 2,
                Host = "localhost",
                Ip = "127.0.0.2",
                PlayerCode = playerCode1,
                RecordOn = DateTime.UtcNow
            },
            new PlayerIpRecord
            {
                IpType = 1,
                Host = "localhost",
                Ip = "127.0.0.3",
                PlayerCode = playerCode2,
                RecordOn = DateTime.UtcNow
            },
            new PlayerIpRecord
            {
                IpType = 2,
                Host = "localhost",
                Ip = "127.0.0.4",
                PlayerCode = playerCode2,
                RecordOn = DateTime.UtcNow
            });

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = playerCode1,
                BrandId = brandId,
                Username = "username1",
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            },
            new Player
            {
                PlayerCode = playerCode2,
                BrandId = brandId,
                Username = "username2",
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.CountCurrentDayLoginPlayersAsync(brandId);

        Assert.AreEqual(2, actual);
    }
}
