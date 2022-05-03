namespace Tgs.PlayerService;

public interface IPlayerRepository
{
    ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyDictionary<string, string>> GetPlayerPropertiesAsync(string playerCode, CancellationToken cancellationToken = default);

    ValueTask SetPlayerPropertiesAsync(
        Guid brandId,
        string playerCode,
        IEnumerable<IDataCommand> modifyCommands,
        CancellationToken cancellationToken = default);
}
