namespace Tgs.PlayerService.ViewModels;

public record UpdatePlayerToAgentViewRequest(
    Guid BrandId,
    Guid AgentId,
    string PlayerCode,
    string CreateByUsername);
