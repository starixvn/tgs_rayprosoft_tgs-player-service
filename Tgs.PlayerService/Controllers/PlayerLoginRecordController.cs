using Microsoft.AspNetCore.Mvc;
using Tgs.PlayerService.ViewModels;
using Tgs.WebApi.Toolkits;

namespace Tgs.PlayerService.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerLoginRecordController : ControllerBase
{
    [HttpGet("group-player", Name = "QueryGroupPlayer")]
    [SuccessStatusCode(200, "PlayerCodes returns")]
    public IAsyncEnumerable<string> QueryGroupPlayerAsync(
        [FromServices] IPlayerLoginRecordRepository repository,
        [FromQuery] PlayerLoginRecordQueryGroupPlayerViewModel source)
        => repository.QueryGroupPlayerAsync(source.StartOn, source.EndOn, source.Page - 1, source.Rows, HttpContext.RequestAborted);
}
