using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class TopUpAccumulatedDepositCommandService(IPlayerRepository playerRepository, ISerialNumberGenerator serialNumberGenerator) : ICommandService<TopUpAccumulatedDepositCommand>
{
    private readonly IPlayerRepository _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    private readonly ISerialNumberGenerator _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));

    public async ValueTask ExecuteAsync(TopUpAccumulatedDepositCommand command, CancellationToken cancellationToken = default)
    {
        await _playerRepository.TopUpAccumulatedDepositAsync(
            command.PlayerCode,
            command.Amount,
            cancellationToken).ConfigureAwait(false);

        await _playerRepository.AddPlayerAuditLogAsync(
            await _serialNumberGenerator.GenerateAsync(
                cancellationToken).ConfigureAwait(false),
            command.PlayerCode,
            "Player",
            "Update Player AccumulatedDeposit",
            command.PlayerCode,
            cancellationToken).ConfigureAwait(false);
    }
}
