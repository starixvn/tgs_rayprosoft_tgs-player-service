namespace Tgs.PlayerService.ApplicationServices;

public record GetPlayerHistoriesQuery(
    string PlayerCode,
    DateTimeOffset From,
    DateTimeOffset To,
    int Skip = 0);
