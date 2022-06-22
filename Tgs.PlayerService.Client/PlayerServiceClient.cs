using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
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

    private readonly HttpClient _httpClient;

    public PlayerServiceClient(HttpClient http)
    {
        _httpClient = http ?? throw new ArgumentNullException(nameof(http));
    }

    public async ValueTask<decimal> GetPlayerDepositedAmountAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .GetAsync($"/player/deposited/{playerCode}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return JsonSerializer.Deserialize<decimal>(content, _SerializerOptions);
    }

    public async ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
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

    public async ValueTask<PlayerInfo?> GetPlayerInfoByEmailAsync(
        Guid brandId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .GetAsync($"/player/{brandId}/{WebUtility.UrlEncode(email)}", cancellationToken)
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

    public async ValueTask<PlayerInfo?> GetPlayerInfoByPhoneAsync(Guid brandId, string areaCode, string phone, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .GetAsync($"/player/{brandId}/{areaCode}/{phone}", cancellationToken)
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

    public async ValueTask<PlayerInfo?> GetPlayerInfoByUserNameAsync(
        Guid brandId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .GetAsync(
                $"/player/{brandId}/{WebUtility.UrlEncode(userName)}",
                cancellationToken)
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

    public async ValueTask<IImmutableDictionary<string, string>> GetPlayerPropertiesAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .GetAsync($"/player/prop/{playerCode}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        return JsonSerializer.Deserialize<IImmutableDictionary<string, string>>(content, _SerializerOptions)!;
    }

    public async ValueTask<PlayerStatus> GetPlayerStatusAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .GetAsync($"/player/status/{playerCode}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<PlayerStatus>(_SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask SetPlayerPropertiesAsync(
        string playerCode,
        IImmutableDictionary<PlayerPropertyName, string?> properties,
        CancellationToken cancellationToken = default)
    {
        var requestContent = new StringContent(
            JsonSerializer.Serialize(properties, _SerializerOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await _httpClient
            .PostAsync($"/player/prop/{playerCode}", requestContent, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
    }
}
