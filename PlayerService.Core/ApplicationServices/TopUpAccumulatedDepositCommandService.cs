using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class TopUpAccumulatedDepositCommandService : ICommandService<TopUpAccumulatedDepositCommand>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ISerialNumberGenerator _serialNumberGenerator;

    public TopUpAccumulatedDepositCommandService(IPlayerRepository playerRepository, ISerialNumberGenerator serialNumberGenerator)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
    }

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
