using System.Text.Json;
using System.Text.Json.Serialization;
using NSubstitute;
using NUnit.Framework;
using Tgs.PlayerService.ApplicationServices;

namespace Tgs.PlayerService.Core.UnitTests;

[TestFixture]
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

        var extraData = new Dictionary<string, string?>
        {
            ["RealName"] = "player1",
            ["IdNumber"] = null,
            ["Wechat"] = null,
            ["QQ"] = null,
            ["AreaCode"] = null,
            ["Phone"] = null,
            ["Email"] = null
        };

        var playerProperties = new Dictionary<string, string>
        {
            ["Viber"] = "ViberId"
        };

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        _ = fakePlayerRepository.GetPlayerExtraDataAsync(playerCode)
            .Returns(extraData);

        _ = fakePlayerRepository.GetPlayerPropertiesAsync(brandId, playerCode)
            .Returns(playerProperties);

        var sut = new GetPlayerPropertiesQueryService(fakePlayerRepository);

        var query = new GetPlayerPropertiesQuery(playerCode);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        CollectionAssert.AreEqual(expected, actual);
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
            Tgs.PlayerService.PlayerStatus.Enabled,
            false,
            null);

        var oldProperties = new Dictionary<string, string>
        {
            ["Viber"] = "viber1",
            ["Telegram"] = "telegram"
        };

        var playerProperties = new Dictionary<string, string?>
        {
            ["Viber"] = null,
            ["Telegram"] = "telegram1",
            ["Line"] = "line123"
        };

        var mergedProperties = new Dictionary<string, string>
        {
            ["Telegram"] = "telegram1",
            ["Line"] = "line123"
        };

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter() }
        };

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        _ = fakePlayerRepository.GetPlayerInfoAsync(playerCode)
            .Returns(playerInfo);

        _ = fakePlayerRepository.GetPlayerPropertiesAsync(brandId, playerCode)
             .Returns(oldProperties);

        var sut = new SetPlayerPropertiesCommandService(fakePlayerRepository);

        var command = new SetPlayerPropertiesCommand(playerCode, playerProperties, jsonSerializerOptions);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakePlayerRepository
             .Received(1)
             .SetPlayerPropertiesAsync(
                 playerInfo.BrandId,
                 playerInfo.PlayerCode,
                 Arg.Is<Dictionary<string, string>>(x =>
                     x.Count == mergedProperties.Count &&
                     x.Keys.All(key => mergedProperties.ContainsKey(key))));
    }
}
