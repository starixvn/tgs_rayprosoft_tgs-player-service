using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class SetPlayerLoginTypeCommandService(
    IPlayerRepository playerRepository,
    ISystemClock systemClock,
    ISerialNumberGenerator serialNumberGenerator) : ICommandService<SetPlayerLoginTypeCommand>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));

    public async ValueTask ExecuteAsync(SetPlayerLoginTypeCommand command, CancellationToken cancellationToken = default)
    {
        await _playerRepository.SetPlayerLoginTypeAsync(
            command.PlayerCode,
            command.LoginType,
            cancellationToken);
    }
}
