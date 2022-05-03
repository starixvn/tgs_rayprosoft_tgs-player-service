using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.Repositories;

internal class PlayerPutDataCommand : PutDataCommand
{
    public PlayerPutDataCommand(string name, string value) : base(name, value)
    {
    }

    public override async ValueTask ExecuteAsync(IDataCommandContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context is not PlayerModifyCommandContext modifyContext)
            throw new PlayerModifyCommandContextTypeErrorException();

        if (Name == nameof(PlayerPropertyName.Birthdate))
            modifyContext.Player.Birthdate = DateOnly.Parse(Value);
        else
        {
            if (modifyContext.PlayerProperties.ContainsKey(Name))
                modifyContext.PlayerProperties[Name].Value = Value;
            else
                _ = await modifyContext.TgsContext.PlayerProperties
                    .AddAsync(new PlayerProperty
                    {
                        PlayerCode = modifyContext.Player.PlayerCode,
                        BrandId = modifyContext.Player.BrandId,
                        Name = Name,
                        Value = Value
                    }, cancellationToken)
                    .ConfigureAwait(false);
        }
    }
}
