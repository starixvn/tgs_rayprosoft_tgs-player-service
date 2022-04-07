using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.PlayerService.Core;
using Tgs.PlayerService.Core.ApplicationServices;
using Tgs.PlayerService.ViewModels;
using Tgs.ServiceUtility;
using Tgs.WebApi.Toolkits;

namespace Tgs.PlayerService.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerController : ControllerBase
{
    [HttpGet("deposited/{playerCode}", Name = "GetPlayerDepositedAmount")]
    [SuccessStatusCode(200, "Player deposited amount returns")]
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
    [SuccessStatusCode(200, "Player info found")]
    [FailStatusCode(404, "Player info found")]
    public async ValueTask<PlayerInfoViewModel?> GetPlayerInfoAsync(
        [FromServices] IGetService<GetPlayerQuery, PlayerInfo?> getService,
        [FromRoute] string playerCode)
    {
        var playerInfo = await getService
            .GetAsync(new GetPlayerQuery(playerCode))
            .ConfigureAwait(false);

        if (playerInfo is not null)
        {
            return new PlayerInfoViewModel(
                playerInfo.PlayerCode,
                playerInfo.BrandId,
                playerInfo.Username,
                playerInfo.IdentityAuthentication,
                playerInfo.PhoneAuthentication,
                playerInfo.Status,
                playerInfo.IsSetPayPassword,
                playerInfo.VipLevelId);
        }

        await this.FailAsync<PlayerInfoViewModel?>(404, null).ConfigureAwait(false);
        return default;
    }

    [HttpGet("prop/{playerCode}", Name = "GetPlayerProperties")]
    [SuccessStatusCode(200, "Player properties returns")]
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

    [HttpPost("/prop/{playerCode}", Name = "SetPlayerProperties")]
    [SuccessStatusCode(200, "Set player properties success")]
    [FailStatusCode(400, "Input properties is empty")]
    public async ValueTask SetPlayerPropertiesAsync(
        [FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
        [FromServices] ICommandService<SetPlayerPropertiesCommand> commandService,
        [FromBody] IReadOnlyDictionary<string, string?> properties,
        [FromRoute] string playerCode)
    {
        if (properties.Count == 0)
        {
            await this.FailAsync(400, "Input properties is empty").ConfigureAwait(false);
        }

        var command = new SetPlayerPropertiesCommand(
            playerCode,
            properties,
            jsonOptionsAccessor.Value.JsonSerializerOptions);

        await commandService.ExecuteAsync(command).ConfigureAwait(false);
    }
}
