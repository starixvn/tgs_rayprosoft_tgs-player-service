// ReSharper disable CheckNamespace

using Microsoft.EntityFrameworkCore;
using Tgs.PlayerService;
using Tgs.PlayerService.Repositories;
using Tgs.PlayerService.Repositories.Entities;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerServiceRepositories(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextConfigure)
        => services
            .AddDbContext<TgsContext>(dbContextConfigure)
            .AddTransient<IPlayerRepository, PlayerRepository>()
            .AddSingleton<IDataCommandBuilder, PlayerDataCommandBuilder>();
}
