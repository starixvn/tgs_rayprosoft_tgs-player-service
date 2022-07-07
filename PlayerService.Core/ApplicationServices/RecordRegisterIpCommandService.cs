using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class RecordRegisterIpCommandService : ICommandService<RecordRegisterIpCommand>
{
    private readonly IPlayerRepository _playerRepository;

    public RecordRegisterIpCommandService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask ExecuteAsync(RecordRegisterIpCommand command, CancellationToken cancellationToken = default)
        => _playerRepository.SaveRegisterIpAsync(
            command.PlayerCode,
            command.Ip,
            command.HostName,
            DateTime.Now,
            cancellationToken);
}
