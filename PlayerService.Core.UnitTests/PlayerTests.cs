using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.Core.Infrastructures.Security;
using Tgs.MarketingReport.Client;
using Tgs.PlayerService.ApplicationServices;
using Tgs.SerialNumberGenerator;

namespace Tgs.PlayerService.Core.UnitTests;

public class PlayerTests
{
    [Test]
    public async Task Can_Get_PlayerDepositedAmount()
    {
        var playerCode = "4ucmqplxb029";

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerDepositedAmountQueryService(fakePlayerRepository);

        var query = new GetPlayerDepositedAmountQuery(playerCode);

        _ = await sut.GetAsync(query).ConfigureAwait(false);

        _ = fakePlayerRepository
            .Received(1)
            .GetPlayerDepositedAmountAsync(playerCode);
    }

    [Test]
    public async Task Can_Get_PlayerInfo()
    {
        var playerCode = "4ucmqplxb029";

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerQueryService(fakePlayerRepository);

        var query = new GetPlayerQuery(playerCode);

        _ = await sut.GetAsync(query).ConfigureAwait(false);

        _ = fakePlayerRepository
            .Received(1)
            .GetPlayerInfoAsync(playerCode);
    }

    [Test]
    public async Task Can_Get_PlayerProperties()
    {
        var playerCode = "player1";

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerPropertiesQueryService(fakePlayerRepository);

        var query = new GetPlayerPropertiesQuery(playerCode);

        _ = await sut.GetAsync(query);

        _ = fakePlayerRepository.Received(1)
            .GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Set_Player_Birthdate()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var playerInfo = new PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
            false,
            false,
            null,
            DateTime.Parse("2022/05/17 00:00:00"));

        var playerProperties = new Dictionary<string, string?>
        {
            ["Birthdate"] = "2022/04/26"
        };

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeDataCommandBuilder = Substitute.For<IDataCommandBuilder>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder, fakeSerialNumberGenerator, jsonOptionsAccessor);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary(), "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5");

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        var eventId = Guid.NewGuid();
        _ = fakeSerialNumberGenerator
            .GenerateAsync()
            .Returns(eventId);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is(nameof(PlayerPropertyName.Birthdate)), Arg.Is("2022/04/26"));

        _ = fakePlayerRepository.Received(1)
            .SetPlayerPropertiesAsync(
                playerInfo.BrandId,
                playerInfo.PlayerCode,
                Arg.Is<IEnumerable<IDataCommand>>(commands => commands.Count() == 1));

        _ = fakePlayerRepository
            .Received(1)
            .CreatePlayerEventAsync(Arg.Is(eventId), Arg.Is(brandId), Arg.Is(playerCode), Arg.Is(EventAction.Update), Arg.Any<JsonDocument>(), Arg.Any<string?>());

        _ = fakePlayerRepository
            .Received(1)
            .UpdatePlayerSnapshotAsync(
                playerCode: Arg.Is(playerCode),
                Arg.Is(eventId),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Set_PlayerProperties()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var playerInfo = new PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
            false,
            false,
            null,
            DateTime.Parse("2022/05/17 00:00:00"));

        var playerProperties = new Dictionary<string, string?>
        {
            ["Viber"] = null,
            ["Telegram"] = "telegram1",
            ["line"] = "line123",
            ["invalid"] = "test"
        };

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeDataCommandBuilder = Substitute.For<IDataCommandBuilder>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder, fakeSerialNumberGenerator, jsonOptionsAccessor);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary(), "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5");

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        var eventId = Guid.NewGuid();
        _ = fakeSerialNumberGenerator
            .GenerateAsync()
            .Returns(eventId);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeDataCommandBuilder.Received(1)
            .CreateDeleteCommand(Arg.Is("Viber"));

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is("Telegram"), Arg.Is("telegram1"));

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is("Line"), Arg.Is("line123"));

        _ = fakePlayerRepository.Received(1)
            .SetPlayerPropertiesAsync(
                playerInfo.BrandId,
                playerInfo.PlayerCode,
                Arg.Is<IEnumerable<IDataCommand>>(commands => commands.Count() == 3));

        _ = fakePlayerRepository
            .Received(1)
            .CreatePlayerEventAsync(Arg.Is(eventId), Arg.Is(brandId), Arg.Is(playerCode), Arg.Is(EventAction.Update), Arg.Any<JsonDocument>(), Arg.Any<string?>());

        _ = fakePlayerRepository
            .Received(1)
            .UpdatePlayerSnapshotAsync(
                playerCode: Arg.Is(playerCode),
                Arg.Is(eventId),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_Player()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakePasswordHandler = Substitute.For<IPasswordHandler>();
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var sut = new CreatePlayerCommandService(
            fakePlayerRepository,
            fakePasswordHandler,
            fakeSystemClock,
            fakeSerialNumberGenerator,
            jsonOptionsAccessor);

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var vipId = Guid.NewGuid();
        var betLimitId = 1;
        var currencyCode = "currencyCode";
        var username = "username";
        var password = "password";
        var agentName = "agentName";
        var eventId = Guid.NewGuid();

        _ = fakeSerialNumberGenerator.GenerateAsync()
            .Returns(eventId);

        var query = new CreatePlayerCommand(
            brandId,
            agentId,
            vipId,
            currencyCode,
            username,
            password,
            null,
            betLimitId,
            PlayerType.RealPlayer,
            RegisterSiteType.Agent,
            agentName);

        _ = await sut.GetAsync(query);

        _ = fakePlayerRepository.Received(1)
            .CreatePlayerAsync(Arg.Any<CreatePlayerData>());

        _ = fakePlayerRepository.Received(1)
            .CreatePlayerTagAsync(Arg.Any<string>(), Arg.Any<string>());

        _ = fakePlayerRepository.Received(1)
            .CreatePlayerEventAsync(
                Arg.Is(eventId),
                Arg.Is(brandId),
                Arg.Any<string>(),
                Arg.Is(EventAction.Create),
                Arg.Any<JsonDocument>(),
                Arg.Any<string?>());

        _ = fakePlayerRepository.Received(1)
            .UpdatePlayerSnapshotAsync(
                Arg.Any<string>(),
                Arg.Is(eventId),
                Arg.Any<DateTime>());
    }

    [Test]
    public async Task Get_Player_Status()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerStatusQueryService(fakePlayerRepository);

        var query = new GetPlayerStatusQuery("player1");

        _ = await sut.GetAsync(query);

        _ = fakePlayerRepository.Received(1)
            .GetPlayerStatusAsync(Arg.Is(query.PlayerCode));
    }

    [Test]
    public async Task Get_PlayerInfo_By_Email()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerInfoByEmailQueryService(fakePlayerRepository);

        var query = new GetPlayerInfoByEmailQuery(Guid.NewGuid(), "tester@test.com");

        _ = await sut.GetAsync(query);

        _ = fakePlayerRepository.Received(1)
            .GetPlayerInfoByEmailAsync(
                Arg.Is(query.BrandId),
                Arg.Is(query.Email));
    }

    [Test]
    public async Task Get_PlayerInfo_By_Phone()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerInfoByPhoneQueryService(fakePlayerRepository);

        var query = new GetPlayerInfoByPhoneQuery(Guid.NewGuid(), "123", "12345678");

        _ = await sut.GetAsync(query);

        _ = fakePlayerRepository.Received(1)
            .GetPlayerInfoByPhoneAsync(
                Arg.Is(query.BrandId),
                Arg.Is(query.AreaCode),
                Arg.Is(query.Phone));
    }

    [Test]
    public async Task Get_PlayerInfo_By_UserName()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerInfoByUserNameQueryService(fakePlayerRepository);

        var query = new GetPlayerInfoByUserNameQuery(Guid.NewGuid(), "userName");

        _ = await sut.GetAsync(query);

        _ = fakePlayerRepository.Received(1)
            .GetPlayerInfoByUserNameAsync(Arg.Is(query.BrandId), Arg.Is(query.UserName));
    }

    [Test]
    public async Task Set_Player_AreaCode_And_Phone()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var playerInfo = new PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
            false,
            false,
            null,
            DateTime.Parse("2022/05/17 00:00:00"));

        var playerProperties = new Dictionary<string, string?>
        {
            ["AreaCode"] = "123",
            ["Phone"] = "12345678",
            ["PhoneAuthentication"] = true.ToString()
        };

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeDataCommandBuilder = Substitute.For<IDataCommandBuilder>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder, fakeSerialNumberGenerator, jsonOptionsAccessor);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary(), "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5");

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        var eventId = Guid.NewGuid();
        _ = fakeSerialNumberGenerator
            .GenerateAsync()
            .Returns(eventId);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is("AreaCode"), Arg.Is("123"));

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is("Phone"), Arg.Is("12345678"));

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is("PhoneAuthentication"), Arg.Is(true.ToString()));

        _ = fakePlayerRepository.Received(1)
            .SetPlayerPropertiesAsync(
                playerInfo.BrandId,
                playerInfo.PlayerCode,
                Arg.Is<IEnumerable<IDataCommand>>(commands => commands.Count() == 3));

        _ = fakePlayerRepository
            .Received(1)
            .CreatePlayerEventAsync(Arg.Is(eventId), Arg.Is(brandId), Arg.Is(playerCode), Arg.Is(EventAction.Update), Arg.Any<JsonDocument>(), Arg.Any<string?>());

        _ = fakePlayerRepository
            .Received(1)
            .UpdatePlayerSnapshotAsync(
                playerCode: Arg.Is(playerCode),
                Arg.Is(eventId),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Set_Player_Email()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var playerInfo = new PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
            false,
            false,
            null,
            DateTime.Parse("2022/05/17 00:00:00"));

        var playerProperties = new Dictionary<string, string?>
        {
            ["Email"] = "tester@test.com"
        };

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeDataCommandBuilder = Substitute.For<IDataCommandBuilder>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder, fakeSerialNumberGenerator, jsonOptionsAccessor);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary(), "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5");

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        var eventId = Guid.NewGuid();
        _ = fakeSerialNumberGenerator
            .GenerateAsync()
            .Returns(eventId);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is("Email"), Arg.Is("tester@test.com"));

        _ = fakePlayerRepository.Received(1)
            .SetPlayerPropertiesAsync(
                playerInfo.BrandId,
                playerInfo.PlayerCode,
                Arg.Is<IEnumerable<IDataCommand>>(commands => commands.Count() == 1));

        _ = fakePlayerRepository
            .Received(1)
            .CreatePlayerEventAsync(Arg.Is(eventId), Arg.Is(brandId), Arg.Is(playerCode), Arg.Is(EventAction.Update), Arg.Any<JsonDocument>(), Arg.Any<string?>());

        _ = fakePlayerRepository
            .Received(1)
            .UpdatePlayerSnapshotAsync(
                playerCode: Arg.Is(playerCode),
                Arg.Is(eventId),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Set_Player_Id_Number()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var playerInfo = new PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
            false,
            false,
            null,
            DateTime.Parse("2022/05/17 00:00:00"));

        var playerProperties = new Dictionary<string, string?>
        {
            ["IdNumber"] = "newIdNumber"
        };

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeDataCommandBuilder = Substitute.For<IDataCommandBuilder>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder, fakeSerialNumberGenerator, jsonOptionsAccessor);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary(), "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5");

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        var eventId = Guid.NewGuid();
        _ = fakeSerialNumberGenerator
            .GenerateAsync()
            .Returns(eventId);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is("IdNumber"), Arg.Is("newIdNumber"));

        _ = fakePlayerRepository.Received(1)
            .SetPlayerPropertiesAsync(
                playerInfo.BrandId,
                playerInfo.PlayerCode,
                Arg.Is<IEnumerable<IDataCommand>>(commands => commands.Count() == 1));

        _ = fakePlayerRepository
            .Received(1)
            .CreatePlayerEventAsync(Arg.Is(eventId), Arg.Is(brandId), Arg.Is(playerCode), Arg.Is(EventAction.Update), Arg.Any<JsonDocument>(), Arg.Any<string?>());

        _ = fakePlayerRepository
            .Received(1)
            .UpdatePlayerSnapshotAsync(
                playerCode: Arg.Is(playerCode),
                Arg.Is(eventId),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Set_Player_Identity_Authentication()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var playerInfo = new PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
            false,
            false,
            null,
            DateTime.Parse("2022/05/17 00:00:00"));

        var playerProperties = new Dictionary<string, string?>
        {
            ["IdentityAuthentication"] = "true"
        };

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeDataCommandBuilder = Substitute.For<IDataCommandBuilder>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();
        var jsonOptionsAccessor = new OptionsWrapper<JsonOptions>(new JsonOptions());

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder, fakeSerialNumberGenerator, jsonOptionsAccessor);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary(), "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5");

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        var eventId = Guid.NewGuid();
        _ = fakeSerialNumberGenerator
            .GenerateAsync()
            .Returns(eventId);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is("IdentityAuthentication"), Arg.Is("true"));

        _ = fakePlayerRepository.Received(1)
            .SetPlayerPropertiesAsync(
                playerInfo.BrandId,
                playerInfo.PlayerCode,
                Arg.Is<IEnumerable<IDataCommand>>(commands => commands.Count() == 1));

        _ = fakePlayerRepository
            .Received(1)
            .CreatePlayerEventAsync(Arg.Is(eventId), Arg.Is(brandId), Arg.Is(playerCode), Arg.Is(EventAction.Update), Arg.Any<JsonDocument>(), Arg.Any<string?>());

        _ = fakePlayerRepository
            .Received(1)
            .UpdatePlayerSnapshotAsync(
                playerCode: Arg.Is(playerCode),
                Arg.Is(eventId),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Set_Player_Status()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var playerCode = "4ucmqplxb029";
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var eventId = Guid.NewGuid();
        var modifier = "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5";

        _ = fakePlayerRepository
            .GetPlayerInfoAsync(playerCode)
            .Returns(new PlayerInfo(playerCode, brandId, "4ucmqplxb029", true, true, true, true, Guid.NewGuid(), DateTime.Now.AddDays(-1)));

        _ = fakeSerialNumberGenerator.GenerateAsync()
            .Returns(eventId);

        var sut = new SetPlayerStatusCommandService(fakePlayerRepository, fakeSystemClock, fakeSerialNumberGenerator);

        var command = new SetPlayerStatusCommand(playerCode, PlayerStatus.Enabled, modifier);

        await sut.ExecuteAsync(command);

        _ = fakePlayerRepository.Received(1)
            .SetPlayerStatusAsync(Arg.Is(command.PlayerCode), Arg.Is(command.Status));

        _ = fakePlayerRepository.Received(1)
            .CreatePlayerEventAsync(
                Arg.Is(eventId),
                Arg.Is(brandId),
                Arg.Is(playerCode),
                Arg.Is(EventAction.Update),
                Arg.Any<JsonDocument>(),
                Arg.Is(modifier));

        _ = fakePlayerRepository.Received(1)
            .UpdatePlayerSnapshotAsync(Arg.Is(playerCode), Arg.Is(eventId), Arg.Any<DateTime>());
    }

    [Test]
    public async Task 用UserNames查詢PlayerCodes()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerCodesByUserNamesQueryService(fakePlayerRepository);

        var query = new GetPlayerCodesByUserNamesQuery(
            Guid.NewGuid(),
            new[] { "player1", "player2" });

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository
            .Received(1)
            .GetPlayerInfosByUserNamesAsync(
                Arg.Is(query.BrandId),
                Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(query.UserNames)));
    }

    [Test]
    public async Task 指定日期註冊玩家人數_查詢日期為前天()
    {
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeMarketingReportClient = Substitute.For<IMarketingReportClient>();

        var sut = new CountReportDateRegisterQueryService(fakeSystemClock, fakePlayerRepository, fakeMarketingReportClient);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var reportDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));

        var now = DateTimeOffset.UtcNow;
        _ = fakeSystemClock.UtcNow.Returns(now);

        _ = fakeMarketingReportClient
            .QueryBrandMarketingStatisticsAsync(brandId, reportDate)
            .Returns(new DailyBrandMarketingStatisticInfo
            {
                BrandId = brandId,
                ReportDate = reportDate,
                DirectlyGamingCount = 1,
                DirectlyRegisterCount = 1,
                TotalGamingCount = 1,
                TotalRegisterCount = 9
            });

        var actual = await sut.GetAsync(new CountReportDateRegisterQuery(brandId, reportDate));

        Assert.AreEqual(9, actual);
    }

    [Test]
    public async Task 指定日期註冊玩家人數_查詢日期為昨天且時間小於排程時間()
    {
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeMarketingReportClient = Substitute.For<IMarketingReportClient>();

        var sut = new CountReportDateRegisterQueryService(fakeSystemClock, fakePlayerRepository, fakeMarketingReportClient);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var reportDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var today = DateTimeOffset.UtcNow;
        _ = fakeSystemClock.UtcNow
            .Returns(new DateTimeOffset(today.Year, today.Month, today.Day, hour: 6, 0, 0, TimeSpan.FromHours(8d)));

        _ = fakePlayerRepository
            .CountYesterdayReigsterAsync(brandId)
            .Returns(6);

        var actual = await sut.GetAsync(new CountReportDateRegisterQuery(brandId, reportDate));

        Assert.AreEqual(6, actual);
    }

    [Test]
    public async Task 指定日期註冊玩家人數_查詢當日()
    {
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeMarketingReportClient = Substitute.For<IMarketingReportClient>();

        var sut = new CountReportDateRegisterQueryService(fakeSystemClock, fakePlayerRepository, fakeMarketingReportClient);

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var reportDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var currentTime = DateTimeOffset.UtcNow;
        _ = fakeSystemClock.UtcNow.Returns(currentTime);

        _ = fakePlayerRepository
            .CurrentDayRegisterAsync(brandId)
            .Returns(3);

        var actual = await sut.GetAsync(new CountReportDateRegisterQuery(brandId, reportDate));

        Assert.AreEqual(3, actual);
    }

    [Test]
    public async Task 查詢ParentPlayer()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetParentPlayerInfoQueryService(fakePlayerRepository);

        var query = new GetParentPlayerInfoQuery("test1");

        _ = fakePlayerRepository.GetParentPlayerCodeAsync(Arg.Is(query.PlayerCode))
            .Returns("parent1");

        _ = await sut.GetAsync(query);

        _ = fakePlayerRepository.Received(1)
            .GetPlayerInfoAsync(Arg.Is("parent1"));
    }

    [Test]
    public async Task 查詢玩家Event_Source()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerHistoriesQueryService(fakePlayerRepository);

        var playerCode = "playerCode";
        var from = DateTime.Now.AddDays(-14);
        var to = DateTime.Now;
        var skip = 10;

        _ = await sut.QueryAsync(new GetPlayerHistoriesQuery(playerCode, from, to, skip)).ToArrayAsync();

        _ = fakePlayerRepository.Received(1)
            .QueryPlayerEventAsync(Arg.Is(new PlayerEventQueryConditions(playerCode, from, to)), Arg.Is(skip));
    }

    [Test]
    public async Task 查詢玩家Event_Source_細項()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerEventQueryService(fakePlayerRepository);

        var eventId = Guid.NewGuid();

        _ = await sut.GetAsync(new GetPlayerEventQuery(eventId));

        _ = fakePlayerRepository.Received(1)
            .GetPlayerEventAsync(Arg.Is(eventId));
    }

    [Test]
    public async Task 查詢玩家Event_Source總筆數()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerHistoriesQueryService(fakePlayerRepository);

        var playerCode = "playerCode";
        var from = DateTime.Now.AddDays(-14);
        var to = DateTime.Now;
        var skip = 10;

        _ = await sut.GetAsync(new GetPlayerHistoriesQuery(playerCode, from, to, skip));

        _ = fakePlayerRepository.Received(1)
            .CountPlayerEventAsync(Arg.Is(new PlayerEventQueryConditions(playerCode, from, to)));
    }

    [Test]
    public async Task 查詢當日註冊玩家人數()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new CurrentDayRegisterQueryService(fakePlayerRepository);

        var brandId = Guid.NewGuid();

        _ = await sut.GetAsync(new CurrentDayRegisterQuery(brandId));

        _ = fakePlayerRepository
            .Received(1)
            .CurrentDayRegisterAsync(Arg.Is(brandId));
    }

    [Test]
    public async Task 變更玩家密碼()
    {
        var fakePasswordHandler = Substitute.For<IPasswordHandler>();
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new SetPlayerPasswordCommandService(
            fakePasswordHandler,
            fakePlayerRepository,
            fakeSerialNumberGenerator);

        var playerCode = "4ucmqplxb029";
        var password = "123456";
        var newPassword = Encoding.UTF8.GetBytes("123456");
        var newSalt = Encoding.UTF8.GetBytes("456789");
        var modifyUsername = "admin.enabled";
        var modifyUserId = "50ce61eb-8c9d-4f84-3cf6-08d9262ce1a5";
        var eventId = Guid.NewGuid();

        _ = fakePasswordHandler
            .GenerateCredential(password)
            .Returns((newSalt, newPassword));

        var playerInfo = new PlayerInfo(
            playerCode,
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            "4ucmqplxb029",
            false,
            false,
            false,
            false,
            Guid.NewGuid(),
            DateTime.Now.AddDays(-1));

        _ = fakePlayerRepository
            .GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(playerInfo);

        _ = fakeSerialNumberGenerator
            .GenerateAsync()
            .Returns(eventId);

        var command = new SetPlayerPasswordCommand(
            playerCode,
            password,
            modifyUsername,
            modifyUserId);

        await sut.ExecuteAsync(command);

        _ = fakePlayerRepository.Received(1)
            .SetPlayerPasswordAsync(Arg.Is(playerCode), Arg.Is(newPassword), Arg.Is(newSalt), Arg.Is(modifyUsername));

        _ = fakePlayerRepository
            .Received(1)
            .CreatePlayerEventAsync(Arg.Is(eventId), Arg.Is(playerInfo.BrandId), Arg.Is(playerCode), Arg.Is(EventAction.Update), Arg.Any<JsonDocument>(), Arg.Is(modifyUserId));

        _ = fakePlayerRepository
            .Received(1)
            .UpdatePlayerSnapshotAsync(Arg.Is(playerCode), Arg.Is(eventId), Arg.Any<DateTime>());
    }
}
