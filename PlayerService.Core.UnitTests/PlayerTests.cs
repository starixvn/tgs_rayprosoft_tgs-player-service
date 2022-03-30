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
    public async Task Can_Get_PlayerDetailInfo()
    {
        var playerCode = "4ucmqplxb029";

        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerPropertiesQueryService(fakePlayerRepository);

        var query = new GetPlayerPropertiesQuery(playerCode);

        _ = await sut.GetAsync(query).ConfigureAwait(false);

        _ = fakePlayerRepository
            .Received(1)
            .GetPlayerPropertiesAsync(playerCode);
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
}
