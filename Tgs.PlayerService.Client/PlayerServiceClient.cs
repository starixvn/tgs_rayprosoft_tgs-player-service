using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.PlayerService.Client;

internal class PlayerServiceClient : IPlayerServiceClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _serializerOptions;

    public PlayerServiceClient(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));

        _serializerOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode)
    {
        var response = await _client
            .GetAsync($"/player/deposited/{playerCode}")
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonSerializer.Deserialize<decimal>(content, _serializerOptions);
    }

    public async ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode)
    {
        var response = await _client
            .GetAsync($"/player/{playerCode}")
            .ConfigureAwait(false);

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return JsonSerializer.Deserialize<PlayerInfo>(content, _serializerOptions);
                }
            case HttpStatusCode.NotFound:
                return null;

            default:
                throw new PlayerServiceApiException("Unknown error");
        }
    }

    public async ValueTask<IReadOnlyDictionary<string, string?>> GetPlayerPropertiesAsync(string playerCode)
    {
        var response = await _client
            .GetAsync($"/player/prop/{playerCode}")
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonSerializer.Deserialize<Dictionary<string, string?>>(content, _serializerOptions)!;
    }
}
