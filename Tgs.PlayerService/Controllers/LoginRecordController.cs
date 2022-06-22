using Microsoft.AspNetCore.Mvc;
using Tgs.PlayerService.ApplicationServices;
using Tgs.PlayerService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.Controllers;

[Route("login-record")]
[ApiController]
public class LoginRecordController : ControllerBase
{
    [HttpPut("{playerCode}", Name = "SaveLoginIp")]
    public ValueTask SaveLoginIpAsync(
        [FromServices] ICommandService<RecordLoginIpCommand> saveService,
        string playerCode,
        [FromBody] IpRecordViewModel ipRecord)
    {
        var command = new RecordLoginIpCommand(
            playerCode,
            ipRecord.Ip,
            ipRecord.HostName);

        return saveService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpPut("register/{playerCode}", Name = "SaveRegisterIp")]
    public ValueTask SaveRegisterIpAsync(
        [FromServices] ICommandService<RecordRegisterIpCommand> saveService,
        string playerCode,
        [FromBody] IpRecordViewModel ipRecord)
    {
        var command = new RecordRegisterIpCommand(
            playerCode,
            ipRecord.Ip,
            ipRecord.HostName);

        return saveService.ExecuteAsync(command, HttpContext.RequestAborted);
    }
}
