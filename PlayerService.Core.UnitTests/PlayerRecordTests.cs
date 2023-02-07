using NSubstitute;
using Tgs.PlayerService.ApplicationServices;

namespace Tgs.PlayerService.Core.UnitTests;

public class PlayerRecordTests
{
    [Test]
    public async Task Record_Login_Ip()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new RecordLoginIpCommandService(fakePlayerRepository);

        var command = new RecordLoginIpCommand(
            "player1",
            "127.0.0.1",
            "localhost");

        var currentTime = DateTime.Now;

        await sut.ExecuteAsync(command);

        _ = fakePlayerRepository.Received(1)
            .SaveLoginIpAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.Ip),
                Arg.Is(command.HostName),
                Arg.Is<DateTime>(time => time >= currentTime));
    }

    [Test]
    public async Task Record_Register_Ip()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new RecordRegisterIpCommandService(fakePlayerRepository);

        var command = new RecordRegisterIpCommand(
            "player1",
            "127.0.0.1",
            "localhost");

        var currentTime = DateTime.Now;

        await sut.ExecuteAsync(command);

        _ = fakePlayerRepository.Received(1)
            .SaveRegisterIpAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.Ip),
                Arg.Is(command.HostName),
                Arg.Is<DateTime>(time => time >= currentTime));
    }

    [Test]
    public async Task 取得玩家的登入記錄資訊()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();

        var sut = new GetPlayerLoginInfoQueryService(fakePlayerRepository);

        var query = new GetPlayerLoginInfoQuery("test1");

        _ = await sut.GetAsync(query);

        _ = fakePlayerRepository.Received(1)
            .GetPlayerLoginInfoAsync(Arg.Is(query.PlayerCode));
    }
}
