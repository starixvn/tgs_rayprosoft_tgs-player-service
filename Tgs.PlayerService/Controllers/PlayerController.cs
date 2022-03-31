using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.PlayerService.Core;
using Tgs.PlayerService.Core.ApplicationServices;
using Tgs.PlayerService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerController : ControllerBase
{
    [HttpGet("deposited/{playerCode}", Name = "GetPlayerDepositedAmount")]
    public async ValueTask<decimal> GetPlayerDepositedAmountAsync(
        [FromServices] IGetService<GetPlayerDepositedAmountQuery, decimal> getService,
        [FromRoute] string playerCode)
    {
        var amount = await getService
            .GetAsync(new GetPlayerDepositedAmountQuery(playerCode))
            .ConfigureAwait(false);

        return amount;
    }

    [HttpGet("{playerCode}", Name = "GetPlayerInfo")]
    public async ValueTask<PlayerViewModel?> GetPlayerInfoAsync(
        [FromServices] IGetService<GetPlayerQuery, PlayerInfo?> getService,
        [FromRoute] string playerCode)
    {
        var playerInfo = await getService
            .GetAsync(new GetPlayerQuery(playerCode))
            .ConfigureAwait(false);

        return playerInfo == null
            ? null
            : new PlayerViewModel(
                playerInfo.PlayerCode,
                playerInfo.BrandId,
                playerInfo.Username,
                playerInfo.IdentityAuthentication,
                playerInfo.PhoneAuthentication,
                playerInfo.Status,
                playerInfo.IsSetPayPassword,
                playerInfo.VipLevelId);
    }

    [HttpGet("prop/{playerCode}", Name = "GetPlayerProperties")]
    public async ValueTask<JsonDocument> GetPlayerPropertiesAsync(
        [FromServices] IGetService<GetPlayerPropertiesQuery, IReadOnlyDictionary<string, string?>> getService,
        [FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
        [FromRoute] string playerCode)
    {
        var jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;

        var properties = await getService
            .GetAsync(new GetPlayerPropertiesQuery(playerCode))
            .ConfigureAwait(false);

        return JsonSerializer.SerializeToDocument(properties, jsonSerializerOptions);
    }
}
