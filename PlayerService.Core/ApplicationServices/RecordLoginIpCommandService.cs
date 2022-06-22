using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class RecordLoginIpCommandService : ICommandService<RecordLoginIpCommand>
{
    private readonly IPlayerRepository _playerRepository;

    public RecordLoginIpCommandService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask ExecuteAsync(RecordLoginIpCommand command, CancellationToken cancellationToken = default)
        => _playerRepository.SaveLoginIpAsync(
            command.PlayerCode,
            command.Ip,
            command.HostName,
            DateTime.Now,
            cancellationToken);
}
