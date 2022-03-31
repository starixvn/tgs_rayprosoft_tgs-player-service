using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Tgs.PlayerService.Client;
using Tgs.PlayerService.Core;

namespace Tgs.PlayerService.IntegrationTests;

[TestFixture]
public class PlayerTests
{
    [Test]
    public async Task Get_Deposited_Amount()
    {
        await using var web = new TestPlayerServiceApplication(
            hostBuilder =>
            {
                _ = hostBuilder.ConfigureServices(services =>
                 {
                     var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                     _ = fakePlayerRepository.GetPlayerDepositedAmountAsync("player1")
                         .Returns(1000m);

                     _ = services.AddSingleton(fakePlayerRepository);
                 });
            });

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
            hostBuilder =>
            {
                _ = hostBuilder.ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
                        .Returns(new Core.PlayerInfo(
                            playerCode,
                            brandId,
                            playerCode,
                            false,
                            false,
                            Core.PlayerStatus.Enabled,
                            false, null));

                    _ = services.AddSingleton(fakePlayerRepository);
                });
            });

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
            hostBuilder =>
            {
                _ = hostBuilder.ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
                        .Returns((Core.PlayerInfo?)null);

                    _ = services.AddSingleton(fakePlayerRepository);
                });
            });

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
            ["Email"] = null
        };

        await using var web = new TestPlayerServiceApplication(
            hostBuilder =>
            {
                _ = hostBuilder.ConfigureServices(services =>
                {
                    var fakePlayerRepository = Substitute.For<IPlayerRepository>();
                    _ = fakePlayerRepository.GetPlayerPropertiesAsync("player1")
                        .Returns(expected);

                    _ = services.AddSingleton(fakePlayerRepository);
                });
            });

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var actual = await sut.GetPlayerPropertiesAsync("player1")
            .ConfigureAwait(false);

        CollectionAssert.AreEqual(expected, actual);
    }
}
