using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class SetPlayerIdentityAuthenticationCommandService : ICommandService<SetPlayerIdentityAuthenticationCommand>
{
    private readonly IPlayerRepository _playerRepository;

    public SetPlayerIdentityAuthenticationCommandService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public ValueTask ExecuteAsync(SetPlayerIdentityAuthenticationCommand command, CancellationToken cancellationToken = default)
        => _playerRepository.SetPlayerIdentityAuthenticationAsync(
            command.PlayerCode,
            command.IdentityAuthentication,
            cancellationToken);
}
