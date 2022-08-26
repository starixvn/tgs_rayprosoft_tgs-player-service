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

        switch (Name)
        {
            case nameof(PlayerPropertyName.Birthdate):
                modifyContext.Player.Birthday = null;
                break;

            case nameof(PlayerPropertyName.AreaCode):
                modifyContext.Player.AreaCode = null;
                break;

            case nameof(PlayerPropertyName.Phone):
                modifyContext.Player.Phone = null;
                break;

            case nameof(PlayerPropertyName.PhoneAuthentication):
                modifyContext.Player.PhoneAuthentication = false;
                break;

            case nameof(PlayerPropertyName.Email):
                modifyContext.Player.Email = null;
                break;

            case nameof(PlayerPropertyName.RealName):
                modifyContext.Player.RealName = null;
                break;

            default:
                if (modifyContext.PlayerProperties.TryGetValue(Name, out var prop))
                    _ = modifyContext.TgsContext.PlayerProperties.Remove(prop);
                break;
        }

        return ValueTask.CompletedTask;
    }
}
