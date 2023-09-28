using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Extensions;

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

    public PlayerServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async ValueTask<int> CountCurrentDayLoginPlayersAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
        .GetAsync($"/login-record/currentday/{brandId}", cancellationToken)
        .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return JsonSerializer.Deserialize<int>(content, _SerializerOptions);
    }

    public async ValueTask<int> CountCurrentDayRegisterPlayersAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
        .GetAsync($"/player/register/currentday/{brandId}", cancellationToken)
        .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return JsonSerializer.Deserialize<int>(content, _SerializerOptions);
    }

    public async ValueTask<int> CountReportDateRegisterPlayersAsync(Guid brandId, DateOnly reportDate, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
        .GetAsync($"/player/register/report-date/{brandId}?date={reportDate}", cancellationToken)
        .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return JsonSerializer.Deserialize<int>(content, _SerializerOptions);
    }

    public async ValueTask<string> CreatePlayerAsync(CreatePlayerData data, CancellationToken cancellationToken = default)
    {
        var requestContent = new StringContent(
            JsonSerializer.Serialize(data, _SerializerOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await _httpClient
            .PostAsync("/player/create", requestContent, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public ValueTask<PlayerInfo?> GetParentPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default)
            => _httpClient
            .GetAsync($"/player/parent/{playerCode}", cancellationToken)
            .ToAsyncEnumerable()
            .Select(response => response.StatusCode switch
            {
                HttpStatusCode.OK => response.Content.ReadFromJsonAsync<PlayerInfo>(_SerializerOptions).ToAsyncEnumerable(),
                HttpStatusCode.NotFound => Task.FromResult<PlayerInfo?>(null).ToAsyncEnumerable(),
                _ => throw new PlayerServiceApiException("Unknown error")
            })
            .Concat()
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

    public async IAsyncEnumerable<string> GetPlayerCodesByUserNamesAsync(
        Guid brandId,
        IEnumerable<string> userNames,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryStrings = new QueryBuilder
        {
            { "names", string.Join(',', userNames) }
        };

        var response = await _httpClient
            .GetAsync($"/player/{brandId}:playercodes{queryStrings.ToQueryString().Value}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        foreach (var item in await response.Content
                     .ReadFromJsonAsync<IEnumerable<string>>(_SerializerOptions, cancellationToken)
                     .ConfigureAwait(false) ?? Array.Empty<string>())
            yield return item;
    }

    public async IAsyncEnumerable<PlayerCountInfo> GetPlayerCountInfosByBrandIdAsync(Guid brandId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var response = await _httpClient
            .GetAsync($"/player/playerCount/{brandId}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        await foreach (var playerCountInfo in JsonSerializer.DeserializeAsyncEnumerable<PlayerCountInfo>(contentStream,
                           _SerializerOptions, cancellationToken)) yield return playerCountInfo!;
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
            .GetAsync($"/player/{brandId}/{Uri.EscapeDataString(areaCode)}/{phone}", cancellationToken)
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

    public ValueTask<PlayerLoginInfo?> GetPlayerLoginInfoAsync(string playerCode, CancellationToken cancellationToken = default)
        => _httpClient.GetAsync($"/login-record/{playerCode}", cancellationToken)
            .ToAsyncEnumerable()
            .Do(response => response.EnsureSuccessStatusCode())
            .Where(response => response.StatusCode != HttpStatusCode.NoContent)
            .SelectMany(response => response.Content
                .ReadFromJsonAsync<PlayerLoginInfo>(_SerializerOptions, cancellationToken)
                .ToAsyncEnumerable())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

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

    public IAsyncEnumerable<string> ListPlayerCodesByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default)
        => _httpClient.GetAsync(
            $"/player/player-code/{brandId}",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken)
            .ToAsyncEnumerable()
            .Do(response => response.EnsureSuccessStatusCode())
            .SelectMany(response => response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ToAsyncEnumerable())
            .SelectMany(stream => JsonSerializer.DeserializeAsyncEnumerable<string>(
                stream,
                _SerializerOptions,
                cancellationToken))
            .Where(playerCode => !string.IsNullOrWhiteSpace(playerCode))
            .Select(playerCode => playerCode!);

    public IAsyncEnumerable<KeyValuePair<string, string>> ListPlayerCodesByUserNameAsync(
        Guid brandId,
        string userNamePrefix,
        CancellationToken cancellationToken = default)
        => _httpClient.GetAsync(
            $"/player/{brandId}:username/{userNamePrefix}",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken)
            .ToAsyncEnumerable()
            .Do(response => response.EnsureSuccessStatusCode())
            .SelectMany(response => response.Content
                .ReadFromJsonAsync<IAsyncEnumerable<KeyValuePair<string, string>>>(
                    _SerializerOptions,
                    cancellationToken)
                .ToAsyncEnumerable()
                .SelectMany(arr => arr!));

    public IAsyncEnumerable<KeyValuePair<string, string>> ListPlayerCodesByUserNamesAsync(
        Guid brandId,
        IEnumerable<string> userNames,
        CancellationToken cancellationToken = default)
    {
        var queryStrings = new QueryBuilder
        {
            { "names", string.Join(',', userNames) }
        };

        return _httpClient
            .GetAsync(
                $"/player/{brandId}:usernames{queryStrings.ToQueryString().Value}",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken)
            .ToAsyncEnumerable()
            .Do(response => response.EnsureSuccessStatusCode())
            .SelectMany(response => response.Content
                .ReadFromJsonAsync<IAsyncEnumerable<KeyValuePair<string, string>>>(
                    _SerializerOptions,
                    cancellationToken)
                .ToAsyncEnumerable()
                .SelectMany(arr => arr!));
    }

    public async IAsyncEnumerable<PlayerInfo> QueryPlayerInfoListAsync(
                Guid brandId,
        IEnumerable<string> playerCodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryStrings = new QueryBuilder
        {
            { "playercodes", string.Join(',', playerCodes) }
        };

        _httpClient.DefaultRequestHeaders.Add("Basic", string.Empty);

        var response = await _httpClient
            .GetAsync($"/player/{brandId}{queryStrings.ToQueryString().Value}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        foreach (var item in await response.Content
                     .ReadFromJsonAsync<IEnumerable<PlayerInfo>>(_SerializerOptions, cancellationToken)
                     .ConfigureAwait(false) ?? Array.Empty<PlayerInfo>())
            yield return item;
    }

    public async IAsyncEnumerable<PlayerListItem> QueryPlayerListAsync(
        Guid brandId,
        PlayerListQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryStrings = new QueryBuilder();

        if (query.Status is not null)
            queryStrings.Add("status", query.Status.Value.ToString());

        if (query.ParentPlayerCode is not null)
            queryStrings.Add("parentPlayerCode", query.ParentPlayerCode);

        if (query.RegisteredDomainName is not null)
            queryStrings.Add("registeredDomainName", query.RegisteredDomainName.Trim());

        if (query.AgentId is not null)
            queryStrings.Add("agentId", query.AgentId.Value.ToString());

        if (query.BeginRegisteredTime is not null)
            queryStrings.Add("beginRegisteredTime", query.BeginRegisteredTime.Value.ToString("o"));

        if (query.EndRegisteredTime is not null)
            queryStrings.Add("endRegisteredTime", query.EndRegisteredTime.Value.ToString("o"));

        if (query.Phone is not null)
            queryStrings.Add("phone", query.Phone);

        if (query.RealName is not null)
            queryStrings.Add("realName", query.RealName);

        if (query.UserName is not null)
            queryStrings.Add("userName", query.UserName);

        if (query.VipId is not null)
            queryStrings.Add("vipId", query.VipId.Value.ToString());

        foreach (var playerCode in query.PlayerCodes)
            queryStrings.Add("playerCode", playerCode);

        var response = await _httpClient
            .GetAsync($"/player/{brandId}{queryStrings.ToQueryString().Value}", cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        foreach (var item in await response.Content
            .ReadFromJsonAsync<IEnumerable<PlayerListItem>>(_SerializerOptions, cancellationToken)
            .ConfigureAwait(false) ?? Array.Empty<PlayerListItem>())
            yield return item;
    }

    public async ValueTask RecordLoginIpAsync(
        string playerCode,
        string ip,
        string? hostName,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            Ip = ip,
            HostName = hostName
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(request, _SerializerOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await _httpClient
            .PutAsync(
                $"/login-record/{playerCode}",
                requestContent,
                cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
    }

    public async ValueTask RecordRegisterIpAsync(
        string playerCode,
        string ip,
        string? hostName,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            Ip = ip,
            HostName = hostName
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(request, _SerializerOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await _httpClient
            .PutAsync(
                $"/login-record/register/{playerCode}",
                requestContent,
                cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
    }

    public async ValueTask SetPlayerPropertiesAsync(
        string playerCode,
        IImmutableDictionary<PlayerPropertyName, string?> properties,
        string? modifyUser = null,
        CancellationToken cancellationToken = default)
    {
        var requestContent = new StringContent(
            JsonSerializer.Serialize(properties, _SerializerOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        if (!string.IsNullOrWhiteSpace(modifyUser))
            requestContent.Headers.Add("ModifyUser", modifyUser);

        var response = await _httpClient
            .PostAsync($"/player/prop/{playerCode}", requestContent, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
    }

    public async ValueTask SetPlayerStatusAsync(
        string playerCode,
        PlayerStatus playerStatus,
        CancellationToken cancellationToken = default)
    {
        var requestContent = new StringContent(
            JsonSerializer.Serialize(playerStatus, _SerializerOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await _httpClient
            .PutAsync($"/player/status/{playerCode}", requestContent, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
    }

    public async ValueTask TopUpAccumulatedDepositAsync(string playerCode, decimal amount, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"/player/deposit/{playerCode}",
            amount,
            cancellationToken).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
    }
}
