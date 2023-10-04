using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using Tgs.PlayerService;
using Tgs.PlayerService.Repositories;
using Tgs.PlayerService.Repositories.Entities;
using Tgs.SerialNumberGenerator;

namespace PlayerService.Repositories.UnitTests;

public class PlayerRepositoryTests
{
    [Test]
    public async Task 更新Player_Snapshot_Id()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));

        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var eventId = Guid.Parse("01852e47-a829-0000-0001-07e6af979b15");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var playerCode = "playerCode";

        var player = new Player
        {
            PlayerCode = playerCode,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            Username = "username",
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>(),
            SnapshotEventId = Guid.Parse("01852e47-a6ee-0000-0001-07e6af979b15")
        };

        _ = await dbContext.Players
            .AddAsync(player)
            .ConfigureAwait(false);

        _ = await dbContext.SaveChangesAsync();

        var sut = new PlayerRepository(dbContext, fakeSerialNumberGenerator);

        await sut.UpdatePlayerSnapshotAsync(
            playerCode,
            eventId,
            DateTime.UtcNow);

        var playerData = await dbContext.Players.FirstAsync(x => x.PlayerCode == playerCode);

        Assert.That(player.SnapshotEventId, Is.EqualTo(eventId));
    }

    [Test]
    public async Task 查詢Player_Event_Source()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));

        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var playerCode = "playerCode";
        var eventId = Guid.NewGuid();

        var playerEvent = new PlayerEventSource
        {
            Id = eventId,
            Action = EventAction.Create.ToString(),
            ChangeSets = "{}",
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            BrandId = Guid.NewGuid(),
            PlayerCode = playerCode,
        };

        _ = await dbContext.PlayerEventSources.AddAsync(playerEvent);

        _ = await dbContext.SaveChangesAsync();

        var sut = new PlayerRepository(dbContext, fakeSerialNumberGenerator);

        var actual = await sut.QueryPlayerEventAsync(new PlayerEventQueryConditions(
            playerCode,
            DateTime.Now.AddDays(-14),
            DateTime.Now),
            0).ToArrayAsync();

        Assert.That(actual, Has.Length.EqualTo(1));
        Assert.That(actual[0].EventId, Is.EqualTo(eventId));
    }

    [Test]
    public async Task 查詢Player_Event_Source細項()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));

        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var eventId = Guid.Parse("018a218f-b4cb-0000-0001-07e7e07dc945");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        _ = await dbContext.PlayerEventSources.AddAsync(new PlayerEventSource
        {
            Id = eventId,
            Action = EventAction.Create.ToString(),
            BrandId = brandId,
            ChangeSets = "{}",
            CreatedOn = DateTime.UtcNow,
            PlayerCode = "playerCode"
        });
        _ = await dbContext.SaveChangesAsync();

        var sut = new PlayerRepository(dbContext, fakeSerialNumberGenerator);

        var actual = await sut.GetPlayerEventAsync(eventId);

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Action, Is.EqualTo(EventAction.Create));
    }

    [Test]
    public async Task 查詢Player_Event_Source總筆數()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));

        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var playerCode = "playerCode";

        var playerEvent = new PlayerEventSource
        {
            Id = Guid.NewGuid(),
            Action = EventAction.Create.ToString(),
            ChangeSets = "{}",
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            BrandId = Guid.NewGuid(),
            PlayerCode = playerCode,
        };

        _ = await dbContext.PlayerEventSources.AddAsync(playerEvent);

        _ = await dbContext.SaveChangesAsync();

        var sut = new PlayerRepository(dbContext, fakeSerialNumberGenerator);

        var actual = await sut.CountPlayerEventAsync(new PlayerEventQueryConditions(
            playerCode,
            DateTime.Now.AddDays(-14),
            DateTime.Now));

        Assert.That(actual, Is.EqualTo(1));
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
    public async Task 新增Player_Event_Source()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
        _ = dbContextOptionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));

        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var eventId = Guid.Parse("018a218f-b4cb-0000-0001-07e7e07dc945");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        var sut = new PlayerRepository(dbContext, fakeSerialNumberGenerator);

        await sut.CreatePlayerEventAsync(
            eventId,
            brandId,
            "playerCode",
            EventAction.Create,
            JsonDocument.Parse("{}"),
            "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5");

        var eventSoruce = await dbContext.PlayerEventSources.FirstAsync(x => x.Id == eventId);

        Assert.That(eventSoruce.Id, Is.EqualTo(eventId));
    }
}
