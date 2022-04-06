// ReSharper disable CheckNamespace

using Microsoft.Extensions.Options;
using Tgs.PlayerService.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerServiceClient(
        this IServiceCollection services,
        Action<PlayerServiceClientOptions, IServiceProvider> configure)
        => services.AddOptions<PlayerServiceClientOptions>()
            .Configure(configure)
            .Services
            .AddHttpClient<IPlayerServiceClient, PlayerServiceClient>()
            .ConfigureHttpClient((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<PlayerServiceClientOptions>>().Value;
                httpClient.BaseAddress = options.BaseUrl;
            })
            .Services;
}
