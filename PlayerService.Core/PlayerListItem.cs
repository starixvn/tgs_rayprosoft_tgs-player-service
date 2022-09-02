namespace Tgs.PlayerService;
public record PlayerListItem(
    string PlayerCode,
    string UserName,
    string? RealName,
    DateTime? RegisteredOn,
    string? RegisteredDomainName,
    Guid? AgentId,
    Guid? VipId,
    PlayerStatus Status,
    string? ParentPlayerCode);
