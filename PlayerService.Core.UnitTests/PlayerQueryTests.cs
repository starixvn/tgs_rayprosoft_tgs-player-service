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

    [Test]
    public async Task 取得Brand下的PlayerCodes()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new BrandAllPlayerCodesQueryService(fakePlayerRepository);

        var query = new BrandAllPlayerCodesQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository.Received(1)
            .QueryPlayerListWithoutLimitAsync(Arg.Is(query.BrandId));
    }

    [Test]
    public async Task 查詢多位玩家的PlayerInfo()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new PlayerInfoListQueryService(fakePlayerRepository);

        var query = new PlayerInfoListQuery(new Guid(), new[] { "player1", "player2" });

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository
            .Received(1)
            .QueryPlayerInfoListAsync(
                Arg.Is(query.BrandId), 
                Arg.Is<IEnumerable<string>>(players => players.SequenceEqual(query.PlayerCodes)),
                Arg.Any<CancellationToken>());
    }
}
