namespace Tgs.PlayerService.Client;

public record CreatePlayerData(
    Guid BrandId,
    Guid? AgentId,
    Guid VipId,
    string CurrencyCode,
    string Username,
    string Password,
    string Email,
    string? UpperPlayerCode,
    int BetLimitId,
    PlayerType PlayerType,
    RegisterSiteType RegisterSiteType,
    string CreateByUsername);
