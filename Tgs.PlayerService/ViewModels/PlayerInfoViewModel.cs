using Tgs.PlayerService.Core;

namespace Tgs.PlayerService.ViewModels;

public record PlayerInfoViewModel(
    string PlayerCode,
    Guid BrandId,
    string Username,
    bool IdentityAuthentication,
    bool PhoneAuthentication,
    PlayerStatus Status,
    bool IsSetPayPassword,
    Guid? VipLevelId);
