namespace Tgs.PlayerService.ApplicationServices;

public record PlayerInfoListQuery(
    Guid BrandId, 
    IEnumerable<string> PlayerCodes,
    PlayerStatus? Status = null,
    Guid? VipId = null,
    string? UserName = null,
    Guid? AgentId = null,
    string? ParentPlayerCode = null,
    string? Phone = null,
    string? RealName = null,
    DateTime? BeginRegisterTime = null,
    DateTime? EndRegisterTime = null);