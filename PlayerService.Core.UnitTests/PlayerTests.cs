using System.Collections.Immutable;
using NSubstitute;
using NUnit.Framework;
using Tgs.Core.Infrastructures.Security;
using Tgs.PlayerService.ApplicationServices;

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

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary());

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is(nameof(PlayerPropertyName.Birthdate)), Arg.Is("2022/04/26"));

        _ = fakePlayerRepository.Received(1)
            .SetPlayerPropertiesAsync(
                playerInfo.BrandId,
                playerInfo.PlayerCode,
                Arg.Is<IEnumerable<IDataCommand>>(commands => commands.Count() == 1));
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

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary());

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

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

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary());

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

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

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository, fakeDataCommandBuilder);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties.ToImmutableDictionary());

        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeDataCommandBuilder.Received(1)
            .CreatePutCommand(Arg.Is("Email"), Arg.Is("tester@test.com"));

        _ = fakePlayerRepository.Received(1)
            .SetPlayerPropertiesAsync(
                playerInfo.BrandId,
                playerInfo.PlayerCode,
                Arg.Is<IEnumerable<IDataCommand>>(commands => commands.Count() == 1));
    }

    [Test]
    public async Task Set_Player_Status()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new SetPlayerStatusCommandService(fakePlayerRepository);

        var command = new SetPlayerStatusCommand("player1", PlayerStatus.Enabled);

        await sut.ExecuteAsync(command);

        _ = fakePlayerRepository.Received(1)
            .SetPlayerStatusAsync(Arg.Is(command.PlayerCode), Arg.Is(command.Status));
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
    public async Task Create_Player()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakePasswordHandler = Substitute.For<IPasswordHandler>();

        var sut = new CreatePlayerQueryService(fakePlayerRepository, fakePasswordHandler);

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var vipId = Guid.NewGuid();
        var betLimitId = 1;
        var currencyCode = "currencyCode";
        var username = "username";
        var password = "password";
        var agentName = "agentName";

        var query = new CreatePlayerQuery(
            brandId,
            agentId,
            vipId,
            currencyCode,
            username,
            password,
            null,
            betLimitId,
            PlayerType.RealPlayer,
            RegisterPlatformType.Agent,
            agentName);

        await sut.GetAsync(query);

        _ = fakePlayerRepository.Received(1)
            .CreatePlayerAsync(Arg.Any<CreatePlayerData>());
    }

}
