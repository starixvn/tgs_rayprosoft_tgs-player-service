namespace Tgs.PlayerService.ApplicationServices;

public record GetPlayerCountByBrandIdQuery(Guid BrandId, bool IncludeUnRegistered);
