namespace Tgs.PlayerService.ApplicationServices;
public record GetPlayerInfoByPhoneQuery(
    Guid BrandId,
    string AreaCode,
    string Phone);
