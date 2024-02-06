using Microsoft.AspNetCore.Mvc;
using Tgs.PlayerService.ViewModels;
using Tgs.WebApi.Toolkits;

namespace Tgs.PlayerService.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerLoginRecordController : ControllerBase
{
    [HttpGet("{brandId:guid}/group-player", Name = "QueryGroupPlayer")]
    [SuccessStatusCode(200, "PlayerCodes returns")]
    public IAsyncEnumerable<string> QueryGroupPlayerAsync(
        [FromServices] IPlayerLoginRecordRepository repository,
        Guid brandId,
        [FromQuery] PlayerLoginRecordQueryGroupPlayerViewModel source)
        => repository.QueryGroupPlayerAsync(brandId, source.StartOn, source.EndOn, source.Page - 1, source.Rows, HttpContext.RequestAborted);
}
