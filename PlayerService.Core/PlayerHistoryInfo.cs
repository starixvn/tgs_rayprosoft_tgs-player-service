namespace Tgs.PlayerService;

public record PlayerHistoryInfo(
    Guid EventId,
    EventAction Action,
    string? Modifier,
    DateTimeOffset CreatedOn);
