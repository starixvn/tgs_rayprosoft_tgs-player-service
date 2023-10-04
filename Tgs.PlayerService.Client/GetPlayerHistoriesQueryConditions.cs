namespace Tgs.PlayerService.Client;

public record GetPlayerHistoriesQueryConditions(
    string PlayerCode,
    DateTime From,
    DateTime To);
