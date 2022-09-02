using NSubstitute;
using NUnit.Framework;
using Tgs.PlayerService.ApplicationServices;

namespace Tgs.PlayerService.Core.UnitTests;

public class PlayerQueryTests
{
    [Test]
    public async Task Query_PlayerList()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new PlayerListQueryService(fakePlayerRepository);

        var query = new PlayerListQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository.Received(1)
            .QueryPlayerListAsync(Arg.Is(query.BrandId));
    }
}
