namespace Tgs.PlayerService;

public record PlayerInfo(
    string PlayerCode,
    Guid BrandId,
    string UserName,
    bool IdentityAuthentication,
    bool PhoneAuthentication,
    PlayerStatus Status,
    bool IsSetPayPassword,
    Guid? VipLevelId);
