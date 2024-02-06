namespace Tgs.PlayerService.ApplicationServices;
public record UpgradeToAgentCommand(
    Guid BrandId,
    Guid AgentId,
    string PlayerCode,
    string? Modifier);
