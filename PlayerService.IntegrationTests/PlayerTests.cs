using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Tgs.PlayerService.Client;

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

        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();

                    _ = fakePlayerRepository
                        .GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
                        .Returns(playerProperties);

                    _ = services.AddSingleton(fakePlayerRepository);
                }));

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var actual = await sut.GetPlayerPropertiesAsync("player1");

        CollectionAssert.AreEqual(walletId.ToString(), actual["DefaultWalletId"]);
    }

    [Test]
    public async Task Get_Deposited_Amount()
    {
        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerDepositedAmountAsync(
                        Arg.Is("player1"),
                        Arg.Any<CancellationToken>())
                        .Returns(1000m);

                    _ = services.AddSingleton(fakePlayerRepository);
                }));

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

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

        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
                            .Returns(new PlayerInfo(
                                playerCode,
                                brandId,
                                playerCode,
                                false,
                                false,
                                PlayerStatus.Enabled,
                                false, null,
                                DateTime.Parse("2022/05/17 00:00:00")));

                    _ = services.AddSingleton(fakePlayerRepository);
                }));

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var actual = await sut.GetPlayerInfoAsync(playerCode)
            .ConfigureAwait(false);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task Get_Player_Info_When_Player_Not_Found()
    {
        var playerCode = "player1";

        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
                        .Returns((Tgs.PlayerService.PlayerInfo?)null);

                    _ = services.AddSingleton(fakePlayerRepository);
                }));

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var actual = await sut.GetPlayerInfoAsync(playerCode)
            .ConfigureAwait(false);

        Assert.IsNull(actual);
    }

    [Test]
    public async Task Get_Player_Properties()
    {
        var expected = new Dictionary<string, string>
        {
            ["RealName"] = "player1",
            ["Viber"] = "ViberId"
        };

        var playerCode = "player1";

        var playerProperties = new Dictionary<string, string>
        {
            ["RealName"] = "player1",
            ["Viber"] = "ViberId"
        };

        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();

                    _ = fakePlayerRepository
                        .GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
                        .Returns(playerProperties);

                    _ = services.AddSingleton(fakePlayerRepository);
                }));

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var actual = await sut.GetPlayerPropertiesAsync("player1");

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public async Task Set_Player_Properties()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var playerInfo = new PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
            PlayerStatus.Enabled,
            false,
            null,
            DateTime.Parse("2022/05/17 00:00:00"));

        var oldProperties = new Dictionary<string, string>
        {
            ["Viber"] = "ViberId",
            ["Line"] = "LineId"
        };

        var playerProperties = new Dictionary<Client.PlayerPropertyName, string?>
        {
            [Client.PlayerPropertyName.Viber] = null,
            [Client.PlayerPropertyName.Line] = "NewLineId",
            [Client.PlayerPropertyName.Telegram] = "TelegramId"
        };

        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
                        .Returns(playerInfo);

                    _ = fakePlayerRepository.GetPlayerPropertiesAsync(playerCode)
                        .Returns(oldProperties);

                    _ = services.AddSingleton(fakePlayerRepository);
                }));

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties)
            .ConfigureAwait(false);
    }
}
