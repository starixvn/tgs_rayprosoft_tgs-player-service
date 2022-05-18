using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.PlayerService.ApplicationServices;
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
    public ValueTask<decimal> GetPlayerDepositedAmountAsync(
        [FromServices] IGetService<GetPlayerDepositedAmountQuery, decimal> getService,
        [FromRoute] string playerCode)
        => getService.GetAsync(
            new GetPlayerDepositedAmountQuery(playerCode),
            HttpContext.RequestAborted);

    [HttpGet("{playerCode}", Name = "GetPlayerInfo")]
    [SuccessStatusCode(200, "Player info found")]
    [FailStatusCode(404, "Player info not found")]
    public async ValueTask<PlayerInfoViewModel> GetPlayerInfoAsync(
        [FromServices] IGetService<GetPlayerQuery, PlayerInfo?> getService,
        [FromRoute] string playerCode)
    {
        var playerInfo = await getService
            .GetAsync(new GetPlayerQuery(playerCode), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return playerInfo is null
            ? await this.FailAsync<PlayerInfoViewModel, object?>(404, null)
                .ConfigureAwait(false)
            : new PlayerInfoViewModel
            {
                PlayerCode = playerInfo.PlayerCode,
                BrandId = playerInfo.BrandId,
                UserName = playerInfo.UserName,
                IdentityAuthentication = playerInfo.IdentityAuthentication,
                PhoneAuthentication = playerInfo.PhoneAuthentication,
                Status = playerInfo.Status,
                IsSetPayPassword = playerInfo.IsSetPayPassword,
                VipLevelId = playerInfo.VipLevelId,
                RegisteredOn = playerInfo.RegisteredOn
            };
    }

    [HttpGet("prop/{playerCode}", Name = "GetPlayerProperties")]
    [SuccessStatusCode(200, "Player properties returns")]
    public async ValueTask<JsonDocument> GetPlayerPropertiesAsync(
        [FromServices] IGetService<GetPlayerPropertiesQuery, IReadOnlyDictionary<string, string>> getService,
        [FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
        [FromRoute] string playerCode)
    {
        var jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;

        var properties = await getService
            .GetAsync(new GetPlayerPropertiesQuery(playerCode), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return JsonSerializer.SerializeToDocument(properties, jsonSerializerOptions);
    }

    [HttpPost("prop/{playerCode}", Name = "SetPlayerProperties")]
    [SuccessStatusCode(200, "Set player properties success")]
    [FailStatusCode(400, "Input properties is empty")]
    public ValueTask SetPlayerPropertiesAsync(
        [FromServices] ICommandService<SetPlayerPropertiesCommand> commandService,
        [FromBody] IReadOnlyDictionary<string, string?> properties,
        [FromRoute] string playerCode)
    {
        if (properties.Count == 0)
            return new ValueTask(this.FailAsync(400, "Input properties is empty"));

        var command = new SetPlayerPropertiesCommand(
            playerCode,
            properties);

        return commandService
            .ExecuteAsync(command, HttpContext.RequestAborted);
    }
}
