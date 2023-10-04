using System.Text.Json;

namespace Tgs.PlayerService;

public record EventInfo(
    EventAction Action,
    JsonDocument ChangeSets,
    string? Modifier,
    DateTime CreatedOn);
