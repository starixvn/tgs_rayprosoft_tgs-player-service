using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.PlayerService.Client;

internal class PlayerServiceClient : IPlayerServiceClient
{
    private static readonly JsonSerializerOptions _SerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public PlayerServiceClient(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetAsync($"/player/deposited/{playerCode}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return JsonSerializer.Deserialize<decimal>(content, _SerializerOptions);
    }

    public async ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetAsync($"/player/{playerCode}", cancellationToken)
            .ConfigureAwait(false);

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                {
                    var content = await response.Content
                        .ReadAsStringAsync(cancellationToken)
                        .ConfigureAwait(false);

                    return JsonSerializer.Deserialize<PlayerInfo>(content, _SerializerOptions);
                }
            case HttpStatusCode.NotFound:
                return null;

            default:
                throw new PlayerServiceApiException("Unknown error");
        }
    }

    public async ValueTask<IReadOnlyDictionary<string, string>> GetPlayerPropertiesAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var response = await _client
            .GetAsync($"/player/prop/{playerCode}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return JsonSerializer.Deserialize<Dictionary<string, string>>(content, _SerializerOptions)!;
    }

    public async ValueTask SetPlayerPropertiesAsync(
        string playerCode,
        IReadOnlyDictionary<PlayerPropertyName, string?> properties,
        CancellationToken cancellationToken = default)
    {
        var requestContent = new StringContent(
            JsonSerializer.Serialize(properties, _SerializerOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await _client
            .PostAsync($"/player/prop/{playerCode}", requestContent, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
    }
}
