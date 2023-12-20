using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class RecordLoginIpCommandService(IPlayerRepository playerRepository) : ICommandService<RecordLoginIpCommand>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public ValueTask ExecuteAsync(RecordLoginIpCommand command, CancellationToken cancellationToken = default)
        => _playerRepository.SaveLoginIpAsync(
            command.PlayerCode,
            command.Ip,
            command.HostName,
            DateTime.Now,
            cancellationToken);
}
