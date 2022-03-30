using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tgs.PlayerService.Core;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.Repositories.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerServiceRepositories(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextConfigure)
        => services
            .AddDbContext<TgsContext>(dbContextConfigure)
            .AddTransient<IPlayerRepository, PlayerRepository>();
}
