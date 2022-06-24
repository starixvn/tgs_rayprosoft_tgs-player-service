using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class SetPlayerStatusCommandService : ICommandService<SetPlayerStatusCommand>
{
    private readonly IPlayerRepository _playerRepository;

    public SetPlayerStatusCommandService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask ExecuteAsync(SetPlayerStatusCommand command, CancellationToken cancellationToken = default)
        => _playerRepository.SetPlayerStatusAsync(
            command.PlayerCode,
            command.Status,
            cancellationToken);
}
