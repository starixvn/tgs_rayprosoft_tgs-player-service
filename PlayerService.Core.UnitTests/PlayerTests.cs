using NSubstitute;
using NUnit.Framework;
using Tgs.PlayerService.Core.ApplicationServices;

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

        var playerInfo = new Core.PlayerInfo(
            playerCode,
            brandId,
            playerCode,
            false,
            false,
            Core.PlayerStatus.Enabled,
            false,
            null);

        var extraData = new Core.PlayerExtraData(
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
}
