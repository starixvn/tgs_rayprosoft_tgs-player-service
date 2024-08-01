namespace Tgs.PlayerService;

public record PlayerInfo(
    string PlayerCode,
    Guid BrandId,
    string UserName,
    bool IdentityAuthentication,
    bool PhoneAuthentication,
    bool EmailAuthentication,
    bool IsSetPayPassword,
    Guid? VipLevelId,
    DateTime? RegisteredOn,
    Guid? AgentId);
