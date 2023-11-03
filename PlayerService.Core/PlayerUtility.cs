using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.PlayerService;

public static class PlayerUtility
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = {
            new JsonStringEnumConverter()
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };
}
