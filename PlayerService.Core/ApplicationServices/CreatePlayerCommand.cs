namespace Tgs.PlayerService.ApplicationServices;
public record CreatePlayerCommand(
    Guid BrandId,
    Guid? AgentId,
    Guid VipId,
    string CurrencyCode,
    string Username,
    string Password,
    string? UpperPlayerCode,
    int BetLimitId,
    PlayerType PlayerType,
    RegisterSiteType RegisterSiteType,
    string CreateByUsername);
