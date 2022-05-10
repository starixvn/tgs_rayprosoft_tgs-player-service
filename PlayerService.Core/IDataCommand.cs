namespace Tgs.PlayerService;

public interface IDataCommand
{
    ValueTask ExecuteAsync(IDataCommandContext context, CancellationToken cancellationToken = default);
}
