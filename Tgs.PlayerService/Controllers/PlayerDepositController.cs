using Microsoft.AspNetCore.Mvc;
using Tgs.PlayerService.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.Controllers;

[Route("player/deposit")]
[ApiController]
public class PlayerDepositController : ControllerBase
{
    [HttpPut("{playerCode}")]
    public ValueTask TopUpAccumulatedDepositAsync(
        [FromServices] ICommandService<TopUpAccumulatedDepositCommand> topUpService,
        string playerCode,
        [FromBody] decimal amount)
    {
        var command = new TopUpAccumulatedDepositCommand(playerCode, amount);

        return topUpService.ExecuteAsync(command, HttpContext.RequestAborted);
    }
}
