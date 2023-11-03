namespace Tgs.PlayerService;

public record CreatePlayerData(
    Guid BrandId,
    string UserName,
    string CurrencyCode,
    int BetLimitId,
    PlayerStatus PlayerStatus,
    PlayerType Type,
    Guid VipId,
    Guid? AgentId,
    string? UpperPlayerCode,
    byte[] Password,
    byte[] Salt);
