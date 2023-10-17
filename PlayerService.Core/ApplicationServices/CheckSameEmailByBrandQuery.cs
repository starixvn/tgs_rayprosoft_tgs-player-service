namespace Tgs.PlayerService.ApplicationServices;

public record CheckSameEmailByBrandQuery(
    Guid BrandId,
    string PlayerCode,
    string Email);
