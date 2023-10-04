namespace Tgs.PlayerService.Client;

public record GetPlayerHistoriesQueryConditions(
    string PlayerCode,
    DateTimeOffset From,
    DateTimeOffset To);
