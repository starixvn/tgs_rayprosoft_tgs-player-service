namespace Tgs.PlayerService.Client;

public class PlayerHistoryInfo
{
    public string Action { get; set; } = default!;

    public DateTimeOffset CreatedOn { get; set; }

    public Guid EventId { get; set; }

    public string? Modifier { get; set; }
}
