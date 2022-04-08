using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class GetPlayerPropertiesQueryService : IGetService<GetPlayerPropertiesQuery, IReadOnlyDictionary<string, string?>>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPlayerPropertiesQueryService(
        IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async ValueTask<IReadOnlyDictionary<string, string?>> GetAsync(
        GetPlayerPropertiesQuery query,
        CancellationToken cancellationToken = default)
    {
        var player = await _playerRepository.GetPlayerInfoAsync(query.PlayerCode)
            .ConfigureAwait(false);

        var properties = new Dictionary<string, string?>();

        if (player == null)
        {
            return properties;
        }

        var extraData = (await _playerRepository.GetPlayerExtraDataAsync(query.PlayerCode)
            .ConfigureAwait(false))!;

        properties["RealName"] = extraData.RealName;
        properties["IdNumber"] = extraData.IdNumber;
        properties["Wechat"] = extraData.WeChat;
        properties["QQ"] = extraData.Qq;
        properties["AreaCode"] = extraData.AreaCode;
        properties["Phone"] = extraData.Phone;
        properties["Email"] = extraData.Email;

        var playerProperties = await _playerRepository
            .GetPlayerPropertiesAsync(player.BrandId, player.PlayerCode)
            .ConfigureAwait(false);

        foreach (var (key, value) in playerProperties)
        {
            if (properties.ContainsKey(key))
                continue;

            properties[key] = value;
        }

        return properties;
    }
}
