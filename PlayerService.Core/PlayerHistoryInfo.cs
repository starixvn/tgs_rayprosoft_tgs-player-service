namespace Tgs.PlayerService;

public record PlayerHistoryInfo(
    Guid EventId,
    string Action,
    string? Modifier,
    DateTimeOffset CreatedOn);
