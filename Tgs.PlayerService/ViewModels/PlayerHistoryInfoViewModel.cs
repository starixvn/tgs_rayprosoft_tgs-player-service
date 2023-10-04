namespace Tgs.PlayerService.ViewModels;

public record PlayerHistoryInfoViewModel(
    Guid EventId,
    string Action,
    string? Modifier,
    DateTimeOffset CreatedOn);
