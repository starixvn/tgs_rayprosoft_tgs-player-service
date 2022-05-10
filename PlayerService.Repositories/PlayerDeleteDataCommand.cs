namespace Tgs.PlayerService.Repositories;

internal class PlayerDeleteDataCommand : DeleteDataCommand
{
    public PlayerDeleteDataCommand(string name) : base(name)
    {
    }

    public override ValueTask ExecuteAsync(IDataCommandContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context is not PlayerModifyCommandContext modifyContext)
            throw new PlayerModifyCommandContextTypeErrorException();

        if (Name == nameof(PlayerPropertyName.Birthdate))
            modifyContext.Player.Birthday = null;
        else if (modifyContext.PlayerProperties.TryGetValue(Name, out var prop))
            _ = modifyContext.TgsContext.PlayerProperties.Remove(prop);

        return ValueTask.CompletedTask;
    }
}
