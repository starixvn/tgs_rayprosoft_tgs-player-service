using System.Text.Json;

namespace Tgs.PlayerService.ApplicationServices;

public record SetPlayerPropertiesCommand(
    string PlayerCode, IReadOnlyDictionary<string, string?> Properties,
    JsonSerializerOptions JsonSerializerOptions);
