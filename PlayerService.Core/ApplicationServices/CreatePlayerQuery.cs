namespace Tgs.PlayerService.ApplicationServices;
public record CreatePlayerQuery(
    Guid BrandId,
    Guid? AgentId,
    Guid VipId,
    string CurrencyCode,
    string Username, 
    string Password,
    int BetLimitId,
    PlayerType PlayerType,
    string CreateByUsername);
