namespace Tgs.PlayerService;

public record PlayerEventQueryConditions(
    string PlayerCode,
    DateTimeOffset From,
    DateTimeOffset To);
