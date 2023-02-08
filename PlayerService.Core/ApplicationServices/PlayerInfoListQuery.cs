namespace Tgs.PlayerService.ApplicationServices;

public record PlayerInfoListQuery(
    IEnumerable<string> PlayerCodes);