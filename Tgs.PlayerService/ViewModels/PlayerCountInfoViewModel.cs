namespace Tgs.PlayerService.ViewModels;

public record PlayerCountInfoViewModel(
    Guid? AgentId,
    int PlayerCount);
