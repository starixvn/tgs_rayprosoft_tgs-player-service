namespace Tgs.PlayerService.ApplicationServices;

public record GetPlayerCodesByUserNamesQuery(
    Guid BrandId,
    IEnumerable<string> UserNames);