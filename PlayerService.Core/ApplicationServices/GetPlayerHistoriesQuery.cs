namespace Tgs.PlayerService.ApplicationServices;

public record GetPlayerHistoriesQuery(
    string PlayerCode,
    DateTime From,
    DateTime To,
    int Skip = 0);
