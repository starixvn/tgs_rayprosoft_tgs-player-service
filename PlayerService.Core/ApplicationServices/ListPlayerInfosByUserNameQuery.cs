namespace Tgs.PlayerService.ApplicationServices;
public record ListPlayerInfosByUserNameQuery(
    Guid BrandId,
    string UserNamePrefix);
