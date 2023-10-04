using System.Text.Json;

namespace Tgs.PlayerService.Client;

public class PlayerEventInfo
{
    public EventAction Action { get; set; }

    public JsonDocument ChangeSets { get; set; } = default!;

    public DateTime CreatedOn { get; set; }

    public string? Modifier { get; set; }
}
