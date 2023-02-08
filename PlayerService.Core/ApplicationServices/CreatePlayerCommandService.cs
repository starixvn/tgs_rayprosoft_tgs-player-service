using System.Collections.Immutable;
using Tgs.Core.Infrastructures.Security;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class CreatePlayerCommandService : IGetService<CreatePlayerCommand, string>
{
    private readonly IPasswordHandler _passwordHandler;
    private readonly IPlayerRepository _playerRepository;

    private readonly IImmutableDictionary<RegisterSiteType, PlayerTagType> _tagMapper =
        new Dictionary<RegisterSiteType, PlayerTagType>
        {
            [RegisterSiteType.Player] = PlayerTagType.PlayerCreated,
            [RegisterSiteType.Agent] = PlayerTagType.AgentCreated,
            [RegisterSiteType.BackendOffice] = PlayerTagType.BackendOfficeCreated
        }.ToImmutableDictionary();

    public CreatePlayerCommandService(
        IPlayerRepository playerRepository,
        IPasswordHandler passwordHandler)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _passwordHandler = passwordHandler ?? throw new ArgumentNullException(nameof(passwordHandler));
    }

    public async ValueTask<string> GetAsync(CreatePlayerCommand command, CancellationToken cancellationToken = default)
    {
        var (salt, password) = _passwordHandler.GenerateCredential(command.Password);

        var playerCode = await _playerRepository
            .CreatePlayerAsync(
                new CreatePlayerData
                {
                    BrandId = command.BrandId,
                    AgentId = command.AgentId,
                    VipId = command.VipId,
                    UserName = command.Username,
                    Salt = salt,
                    Password = password,
                    CurrencyCode = command.CurrencyCode,
                    BetLimitId = command.BetLimitId,
                    Type = command.PlayerType,
                    PlayerStatus = PlayerStatus.RegisterUndone,
                    UpperPlayerCode = command.UpperPlayerCode
                }, cancellationToken)
            .ConfigureAwait(false);

        await _playerRepository
            .CreatePlayerTagAsync(playerCode, _tagMapper[command.RegisterSiteType].ToString(), cancellationToken)
            .ConfigureAwait(false);

        return playerCode;
    }
}
