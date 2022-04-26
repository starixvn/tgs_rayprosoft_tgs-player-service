namespace Tgs.PlayerService;

public abstract class PutDataCommand : IDataCommand
{
    public string Name { get; }

    public string Value { get; }

    public PutDataCommand(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public abstract ValueTask ExecuteAsync(IDataCommandContext context, CancellationToken cancellationToken = default);
}
