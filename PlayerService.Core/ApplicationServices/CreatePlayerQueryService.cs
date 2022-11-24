using DocumentFormat.OpenXml.VariantTypes;
using Tgs.Core.Infrastructures.Security;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class CreatePlayerQueryService : IGetService<CreatePlayerQuery, string>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPasswordHandler _passwordHandler;

    public CreatePlayerQueryService(
        IPlayerRepository playerRepository, 
        IPasswordHandler passwordHandler)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _passwordHandler = passwordHandler ?? throw new ArgumentNullException(nameof(passwordHandler));
    }

    public async ValueTask<string> GetAsync(CreatePlayerQuery query, CancellationToken cancellationToken = default)
    {
        var (salt, password) = _passwordHandler.GenerateCredential(query.Password);

        return await _playerRepository
            .CreatePlayerAsync(
                new CreatePlayerData
                {
                    BrandId = query.BrandId,
                    AgentId= query.AgentId,
                    VipId = query.VipId,
                    UserName = query.Username,
                    Salt = salt,
                    Password = password,
                    CurrencyCode = query.CurrencyCode,
                    BetLimitId = query.BetLimitId,
                    Type = query.PlayerType,
                    PlayerStatus = PlayerStatus.Enabled
                }, cancellationToken)
            .ConfigureAwait(false);
    }
}
