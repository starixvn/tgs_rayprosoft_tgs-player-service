namespace Tgs.PlayerService.ViewModels;

public record PlayerHistoryInfoViewModel(
    Guid EventId,
    EventAction Action,
    string? Modifier,
    DateTimeOffset CreatedOn);
