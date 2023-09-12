using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using Tgs.PlayerService.Repositories;
using Tgs.PlayerService.Repositories.Entities;
using Tgs.SerialNumberGenerator;

namespace PlayerService.Repositories.UnitTests;

public class PlayerRepositoryTests
{
    [Test]
    public async Task 查詢當日登入總人數()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));

        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new PlayerRepository(dbContext, fakeSerialNumberGenerator);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var playerCode1 = "playerCode1";
        var playerCode2 = "playerCode2";

        await dbContext.PlayerIpRecords.AddRangeAsync(
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

        var actual = await sut.CountCurrentDayLoginAsync(brandId);

        Assert.That(actual, Is.EqualTo(2));
    }

    [Test]
    public async Task 查詢當日登入總人數_非當日紀錄不列入()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));

        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new PlayerRepository(dbContext, fakeSerialNumberGenerator);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var playerCode1 = "playerCode1";
        var playerCode2 = "playerCode2";

        await dbContext.PlayerIpRecords.AddRangeAsync(
            new PlayerIpRecord
            {
                IpType = 2,
                Host = "localhost",
                Ip = "127.0.0.2",
                PlayerCode = playerCode1,
                RecordOn = DateTime.UtcNow.AddDays(-1)
            },
            new PlayerIpRecord
            {
                IpType = 2,
                Host = "localhost",
                Ip = "127.0.0.4",
                PlayerCode = playerCode2,
                RecordOn = DateTime.UtcNow.AddDays(-1)
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

        var actual = await sut.CountCurrentDayLoginAsync(brandId);

        Assert.That(actual, Is.EqualTo(0));
    }

    [Test]
    public async Task 查詢當日註冊人數()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));

        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new PlayerRepository(dbContext, fakeSerialNumberGenerator);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var playerCode1 = "playerCode1";
        var playerCode2 = "playerCode2";

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
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.UtcNow
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
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.UtcNow
            });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.CurrentDayRegisterAsync(brandId);

        Assert.That(actual, Is.EqualTo(2));
    }

    [Test]
    public async Task 查詢昨日註冊人數()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));

        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var playerCode1 = "playerCode1";
        var playerCode2 = "playerCode2";

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
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.UtcNow.AddDays(-1)
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
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.UtcNow.AddDays(-1)
            });

        _ = await dbContext.SaveChangesAsync();

        var sut = new PlayerRepository(dbContext, fakeSerialNumberGenerator);

        var actual = await sut.CountYesterdayReigsterAsync(brandId);

        Assert.That(actual, Is.EqualTo(2));
    }
}
