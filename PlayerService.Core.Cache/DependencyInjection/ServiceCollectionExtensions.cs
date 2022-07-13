using System.Collections.Immutable;
using PlayerService.Core.Cache;
using Tgs.PlayerService;
using Tgs.PlayerService.ApplicationServices;
using Tgs.ServiceUtility;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerServiceCache(this IServiceCollection services)
        => services;
            //.Decorate<IGetService<GetPlayerInfoByEmailQuery, PlayerInfo?>, GetCachedPlayerInfoByEmailQueryService>()
            //.Decorate<IGetService<GetPlayerInfoByPhoneQuery, PlayerInfo?>, GetCachedPlayerInfoByPhoneQueryService>()
            //.Decorate<IGetService<GetPlayerInfoByUserNameQuery, PlayerInfo?>, GetCachedPlayerInfoByUserNameQueryService>()
            //.Decorate<IGetService<GetPlayerPropertiesQuery, IImmutableDictionary<string, string>>, GetCachedPlayerPropertiesQueryService>()
            //.Decorate<IGetService<GetPlayerQuery, PlayerInfo?>, GetCachedPlayerQueryService>()
            //.Decorate<ICommandService<SetPlayerPropertiesCommand>, SetPlayerPropertiesCachedProcessCommandService>();
}
