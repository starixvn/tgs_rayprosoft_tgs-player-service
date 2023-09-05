using Microsoft.AspNetCore.Mvc;
using Tgs.PlayerService.ApplicationServices;
using Tgs.PlayerService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.Controllers;

[Route("login-record")]
[ApiController]
public class LoginRecordController : ControllerBase
{
    [HttpGet("currentday/{brandId}", Name = "CountCurrentDayLogin")]
    public ValueTask<int> CountCurrentDayLoginAsync(
        [FromServices] IGetService<CountCurrentDayLoginQuery, int> getService,
        Guid brandId)
    {
        var query = new CountCurrentDayLoginQuery(brandId);

        return getService.GetAsync(query, HttpContext.RequestAborted);
    }

    [HttpGet("{playerCode}", Name = "GetLoginInfo")]
    public ValueTask<PlayerLoginInfoViewModel?> GetLoginInfoAsync(
        [FromServices] IGetService<GetPlayerLoginInfoQuery, PlayerLoginInfo?> getService,
        string playerCode)
    {
        var query = new GetPlayerLoginInfoQuery(playerCode);

        return getService.GetAsync(query, HttpContext.RequestAborted)
            .AsTask()
            .ToAsyncEnumerable()
            .Where(info => info is not null)
            .Select(info => new PlayerLoginInfoViewModel
            {
                RegisterIp = info!.RegisterIp,
                RegisterHost = info!.RegisterHost,
                LoginIp = info!.LoginIp,
                LoginOn = info!.LoginOn
            })
            .FirstOrDefaultAsync(HttpContext.RequestAborted);
    }

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
