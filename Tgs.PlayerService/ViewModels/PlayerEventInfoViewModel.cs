using System.Text.Json;

namespace Tgs.PlayerService.ViewModels;

public class PlayerEventInfoViewModel
{
    public EventAction Action { get; set; }

    public JsonDocument ChangeSets { get; set; } = default!;

    public DateTime CreatedOn { get; set; }

    public string? Modifier { get; set; }
}
