using NSubstitute;
using Tgs.PlayerService.ApplicationServices;

namespace Tgs.PlayerService.Core.UnitTests;

public class PlayerQueryTests
{
    [Test]
    public async Task Query_PlayerList()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new PlayerListQueryService(fakePlayerRepository);

        var query = new PlayerListQuery(Guid.NewGuid(), Array.Empty<string>());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository.Received(1)
            .QueryPlayerListAsync(Arg.Is(query.BrandId));
    }

    [Test]
    public async Task Query_PlayerList_By_Players()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new PlayerListQueryService(fakePlayerRepository);

        var query = new PlayerListQuery(Guid.NewGuid(), new[] { "player1", "player2" });

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository.Received(1)
            .QueryPlayerListByPlayersAsync(
                Arg.Is(query.BrandId),
                Arg.Is<IEnumerable<string>>(players => Enumerable.SequenceEqual(players, query.PlayerCodes)));
    }
}
