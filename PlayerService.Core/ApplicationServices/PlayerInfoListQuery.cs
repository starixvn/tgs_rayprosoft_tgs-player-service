namespace Tgs.PlayerService.ApplicationServices;

public record PlayerInfoListQuery(Guid BrandId, IEnumerable<string> PlayerCodes);