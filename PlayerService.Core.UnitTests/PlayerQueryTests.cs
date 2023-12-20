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

        var playerCodes = new[] { "player1", "player2" };
        var query = new PlayerListQuery(Guid.NewGuid(), playerCodes);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository.Received(1)
            .QueryPlayerListByPlayersAsync(
                Arg.Is(query.BrandId),
                Arg.Is<IEnumerable<string>>(players => Enumerable.SequenceEqual(players, query.PlayerCodes)));
    }

    [Test]
    public async Task 用UserNames查詢PlayerInfos()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new ListPlayerInfosByUserNamesQueryService(fakePlayerRepository);

        var userNames = new[] { "player1", "player2" };
        var query = new ListPlayerInfosByUserNamesQuery(
            Guid.NewGuid(),
            userNames);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository.Received(1)
            .GetPlayerInfosByUserNamesAsync(
                Arg.Is(query.BrandId),
                Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(query.UserNames)));
    }

    [Test]
    public async Task 依據UserNamePrefix查詢玩家列表()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new ListPlayerInfosByUserNameQueryService(fakePlayerRepository);

        var query = new ListPlayerInfosByUserNameQuery(Guid.NewGuid(), "test");

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository.Received(1)
            .QueryPlayerInfosByUserNameAsync(
                Arg.Is(query.BrandId),
                Arg.Is(query.UserNamePrefix));
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

        var playerCodes = new[] { "player1", "player2" };
        var query = new PlayerInfoListQuery(Guid.NewGuid(), playerCodes);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerRepository
            .Received(1)
            .QueryPlayerInfoListAsync(
                Arg.Is(query.BrandId),
                Arg.Is<IEnumerable<string>>(players => players.SequenceEqual(query.PlayerCodes)),
                Arg.Any<CancellationToken>());
    }
}
