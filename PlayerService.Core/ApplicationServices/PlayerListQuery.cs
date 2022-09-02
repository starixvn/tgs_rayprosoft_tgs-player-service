namespace Tgs.PlayerService.ApplicationServices;
public record PlayerListQuery(
    Guid BrandId,
    PlayerStatus? Status = null,
    Guid? VipId = null,
    string? UserName = null,
    Guid? AgentId = null,
    string? ParentPlayerCode = null,
    string? Phone = null,
    string? RealName = null,
    string? RegisteredDomainName = null,
    DateTime? BeginRegisterTime = null,
    DateTime? EndRegisterTime = null);
