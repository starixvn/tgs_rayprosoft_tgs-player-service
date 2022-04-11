using System;
using System.Collections.Generic;
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
    public async Task Get_Deposited_Amount()
    {
        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerDepositedAmountAsync("player1")
                        .Returns(1000m);

                    _ = services.AddSingleton(fakePlayerRepository);
                }));

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var actual = await sut.GetPlayerDepositedAmountAsync("player1")
            .ConfigureAwait(false);

        Assert.Greater(actual, 0);
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
            Username = playerCode,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            IsSetPayPassword = false,
            VipLevelId = null,
            Status = Client.PlayerStatus.Enabled
        };

        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
                        .Returns(new Tgs.PlayerService.PlayerInfo(
                            playerCode,
                            brandId,
                            playerCode,
                            false,
                            false,
Tgs.PlayerService.PlayerStatus.Enabled,
                            false, null));

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
        var expected = new Dictionary<string, string?>
        {
            ["RealName"] = "player1",
            ["IdNumber"] = null,
            ["Wechat"] = null,
            ["QQ"] = null,
            ["AreaCode"] = null,
            ["Phone"] = null,
            ["Email"] = null,
            ["Viber"] = "ViberId"
        };

        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var playerInfo = new Tgs.PlayerService.PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
Tgs.PlayerService.PlayerStatus.Enabled,
            false,
            null);

        var extraData = new Tgs.PlayerService.PlayerExtraData(
            "player1",
            null,
            null,
            null,
            null,
            null,
            null);

        var playerProperties = new Dictionary<string, string>
        {
            ["Viber"] = "ViberId"
        };

        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
                        .Returns(playerInfo);

                    _ = fakePlayerRepository.GetPlayerExtraDataAsync(playerCode)
                        .Returns(extraData);

                    _ = fakePlayerRepository.GetPlayerPropertiesAsync(brandId, playerCode)
                        .Returns(playerProperties);

                    _ = services.AddSingleton(fakePlayerRepository);
                }));

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var actual = await sut.GetPlayerPropertiesAsync("player1")
            .ConfigureAwait(false);

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public async Task Set_Player_Properties()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "player1";

        var playerInfo = new Tgs.PlayerService.PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
Tgs.PlayerService.PlayerStatus.Enabled,
            false,
            null);

        var oldProperties = new Dictionary<string, string>
        {
            ["Viber"] = "ViberId",
            ["Line"] = "LineId"
        };

        var playerProperties = new Dictionary<PlayerPropertyName, string?>
        {
            [PlayerPropertyName.Viber] = null,
            [PlayerPropertyName.Line] = "NewLineId",
            [PlayerPropertyName.Telegram] = "TelegramId"
        };

        await using var web = new TestPlayerServiceApplication(
            hostBuilder => hostBuilder
                .ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
                        .Returns(playerInfo);

                    _ = fakePlayerRepository.GetPlayerPropertiesAsync(brandId, playerCode)
                        .Returns(oldProperties);

                    _ = services.AddSingleton(fakePlayerRepository);
                }));

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        await sut.SetPlayerPropertiesAsync(playerCode, playerProperties)
            .ConfigureAwait(false);
    }
}
