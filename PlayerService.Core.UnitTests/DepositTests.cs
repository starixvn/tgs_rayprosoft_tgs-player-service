using NSubstitute;
using Tgs.PlayerService.ApplicationServices;
using Tgs.SerialNumberGenerator;

namespace Tgs.PlayerService.Core.UnitTests;

public class DepositTests
{
    [Test]
    public async Task 更新玩家總存款金額()
    {
        var fakePlayerRepository = Substitute.For<IPlayerRepository>();
        var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

        var sut = new TopUpAccumulatedDepositCommandService(
            fakePlayerRepository,
            fakeSerialNumberGenerator);

        var command = new TopUpAccumulatedDepositCommand("test1", 1000M);

        var createId = Guid.NewGuid();

        _ = fakeSerialNumberGenerator.GenerateAsync()
            .Returns(createId);

        await sut.ExecuteAsync(command);

        _ = fakePlayerRepository.Received(1)
            .TopUpAccumulatedDepositAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.Amount));
    }
}
