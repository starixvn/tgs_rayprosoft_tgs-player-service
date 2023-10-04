namespace Tgs.PlayerService;

public record PlayerEventQueryConditions(
    string PlayerCode,
    DateTime From,
    DateTime To);
