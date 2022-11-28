namespace Tgs.PlayerService.ViewModels;

public record CreatePlayerViewModel(
    Guid BrandId,
    Guid? AgentId,
    Guid VipId,
    string CurrencyCode,
    string Username,
    string Password,
    int BetLimitId,
    PlayerType PlayerType,
    RegisterPlatformType RegisterPlatformType,
    string CreateByUsername);
