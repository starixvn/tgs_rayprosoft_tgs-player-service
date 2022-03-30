using Tgs.PlayerService.Core;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.Repositories;

internal class PlayerRepository : IPlayerRepository
{
    private readonly TgsContext _context;

    public PlayerRepository(
        TgsContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async ValueTask<decimal?> GetPlayerDepositedAmountAsync(string playerCode)
    {
        var player = await _context.Players
            .FindAsync(playerCode)
            .ConfigureAwait(false);

        return player?.AccumulatedDeposit;
    }

    public ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode)
    {
        throw new NotImplementedException();
    }

    public ValueTask<IReadOnlyDictionary<string, string?>?> GetPlayerPropertiesAsync(string playerCode)
    {
        throw new NotImplementedException();
    }
}
