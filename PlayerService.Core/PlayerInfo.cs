namespace Tgs.PlayerService.Core;

public record PlayerInfo(
    string PlayerCode,
    Guid BrandId,
    string Username,
    bool IdentityAuthentication,
    bool PhoneAuthentication,
    PlayerStatus Status,
    bool IsSetPayPassword,
    Guid? VipLevelId);
