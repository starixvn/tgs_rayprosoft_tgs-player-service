using System.Text.Json;

namespace Tgs.PlayerService.Core.ApplicationServices;

public record SetPlayerPropertiesCommand(
    string PlayerCode, IReadOnlyDictionary<string, string?> Properties,
    JsonSerializerOptions JsonSerializerOptions);
