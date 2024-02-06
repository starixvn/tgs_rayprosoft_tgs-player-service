using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class UpgradeToAgentCommandService(IPlayerRepository playerRepository) : ICommandService<UpgradeToAgentCommand>
{
    public async ValueTask ExecuteAsync(UpgradeToAgentCommand command, CancellationToken cancellationToken = default)
    {
        await playerRepository.CreatePlayerAgentRelationAsync(
            command.BrandId,
            command.AgentId,
            command.PlayerCode,
            cancellationToken);
    }
}
