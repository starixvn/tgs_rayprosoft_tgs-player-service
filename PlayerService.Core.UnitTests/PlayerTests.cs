using NSubstitute;
using NUnit.Framework;
using Tgs.PlayerService.Core.ApplicationServices;

namespace Tgs.PlayerService.Core.UnitTests;

[TestFixture]
public class PlayerTests
{
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
