namespace Tgs.PlayerService.ApplicationServices;

public record ListPlayerInfosByUserNamesQuery(
    Guid BrandId,
    IEnumerable<string> UserNames);
