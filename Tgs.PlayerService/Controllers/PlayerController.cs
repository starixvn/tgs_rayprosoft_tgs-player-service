using Microsoft.AspNetCore.Mvc;
using Tgs.PlayerService.Core;
using Tgs.PlayerService.Core.ApplicationServices;
using Tgs.PlayerService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerController : ControllerBase
{
    [HttpGet("{playerCode}/deposited_amount", Name = "GetPlayerDepositedAmount")]
    public async ValueTask<decimal?> GetPlayerDepositedAmountAsync(
        [FromServices] IGetService<GetPlayerDepositedAmountQuery, decimal?> getService,
        [FromRoute] string playerCode)
    {
        var amount = await getService.GetAsync(new GetPlayerDepositedAmountQuery(playerCode))
            .ConfigureAwait(false);

        return amount;
    }

    [HttpGet("{playerCode}/detail", Name = "GetPlayerDetailInfo")]
    public async ValueTask<PlayerDetailViewModel> GetPlayerDetailInfoAsync(
        [FromServices] IGetService<GetPlayerDetailQuery, PlayerDetailInfo> getService,
        [FromRoute] string playerCode)
    {
        var playerInfo = await getService.GetAsync(new GetPlayerDetailQuery(playerCode))
            .ConfigureAwait(false);

        return new PlayerDetailViewModel(
            playerInfo.PlayerCode,
            playerInfo.Properties);
    }

    [HttpGet("{playerCode}", Name = "GetPlayerInfo")]
    public async ValueTask<PlayerViewModel> GetPlayerInfoAsync(
        [FromServices] IGetService<GetPlayerQuery, PlayerInfo> getService,
        [FromRoute] string playerCode)
    {
        var playerInfo = await getService.GetAsync(new GetPlayerQuery(playerCode))
            .ConfigureAwait(false);

        return new PlayerViewModel(
            playerInfo.PlayerCode,
            playerInfo.BrandId,
            playerInfo.Username,
            playerInfo.IdentityAuthentication,
            playerInfo.PhoneAuthentication,
            playerInfo.Status,
            playerInfo.IsSetPayPassword,
            playerInfo.VipLevelId);
    }
}
