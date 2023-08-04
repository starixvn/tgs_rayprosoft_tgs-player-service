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

        switch (Name)
        {
            case nameof(PlayerPropertyName.Birthdate):
                modifyContext.Player.Birthdate = DateOnly.Parse(Value);
                break;

            case nameof(PlayerPropertyName.AreaCode):
                modifyContext.Player.AreaCode = Value;
                break;

            case nameof(PlayerPropertyName.Phone):
                modifyContext.Player.Phone = Value;
                break;

            case nameof(PlayerPropertyName.PhoneAuthentication):
                modifyContext.Player.PhoneAuthentication = bool.Parse(Value);
                break;

            case nameof(PlayerPropertyName.Email):
                modifyContext.Player.Email = Value;
                break;

            case nameof(PlayerPropertyName.RealName):
                modifyContext.Player.RealName = Value;
                break;

            case nameof(PlayerPropertyName.Remark):
                modifyContext.Player.Remark = Value;
                break;

            case nameof(PlayerPropertyName.IdentityAuthentication):
                modifyContext.Player.IdentityAuthentication = bool.Parse(Value);
                break;

            case nameof(PlayerPropertyName.IdNumber):
                modifyContext.Player.IdNumber = Value;
                break;

            default:
                if (modifyContext.PlayerProperties.TryGetValue(Name, out var value))
                    value.Value = Value;
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
                break;
        }
    }
}
