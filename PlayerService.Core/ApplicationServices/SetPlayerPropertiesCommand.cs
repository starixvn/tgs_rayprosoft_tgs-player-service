using System.Collections.Immutable;

namespace Tgs.PlayerService.ApplicationServices;

public record SetPlayerPropertiesCommand(
    string PlayerCode,
    IImmutableDictionary<string, string?> Properties,
    string? Modifier);
