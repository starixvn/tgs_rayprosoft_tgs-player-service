using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Paging;
using Tgs.PlayerService.ApplicationServices;
using Tgs.PlayerService.ViewModels;
using Tgs.ServiceUtility;
using Tgs.WebApi.Toolkits;

namespace Tgs.PlayerService.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerController : ControllerBase
{
    [HttpPost("create", Name = "CreatePlayer")]
    [SuccessStatusCode(200, "Create player")]
    public ValueTask<string> CreatePlayerAsync(
        [FromServices] IGetService<CreatePlayerCommand, string> queryService,
        [FromBody] CreatePlayerViewModel player)
    {
        var command = new CreatePlayerCommand(
            player.BrandId,
            player.AgentId,
            player.VipId,
            player.CurrencyCode,
            player.Username,
            player.Password,
            player.UpperPlayerCode,
            player.BetLimitId,
            player.PlayerType,
            player.RegisterSiteType,
            player.CreateByUsername);

        return queryService.GetAsync(command, HttpContext.RequestAborted);
    }

    [HttpGet("parent/{playerCode}", Name = "GetParentPlayerInfo")]
    [SuccessStatusCode(200, "parent player info found")]
    [FailStatusCode(404, "parent player info not found")]
    public async Task<PlayerInfoViewModel> GetParentPlayerInfoAsync(
        [FromServices] IGetService<GetParentPlayerInfoQuery, PlayerInfo?> getService,
        string playerCode)
    {
        var query = new GetParentPlayerInfoQuery(playerCode);

        var parentPlayer = await getService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return parentPlayer is null
            ? await this.FailAsync<PlayerInfoViewModel, string>(
                404,
                "Not found").ConfigureAwait(false)
            : new PlayerInfoViewModel
            {
                PlayerCode = parentPlayer.PlayerCode,
                BrandId = parentPlayer.BrandId,
                UserName = parentPlayer.UserName,
                IdentityAuthentication = parentPlayer.IdentityAuthentication,
                PhoneAuthentication = parentPlayer.PhoneAuthentication,
                EmailAuthentication = parentPlayer.EmailAuthentication,
                IsSetPayPassword = parentPlayer.IsSetPayPassword,
                VipLevelId = parentPlayer.VipLevelId,
                RegisteredOn = parentPlayer.RegisteredOn
            };
    }

    [HttpGet("playerCodes/{brandId:Guid}", Name = "GetPlayerCodesByUserNames")]
    [SuccessStatusCode(200, "PlayerCodes returns")]
    public IAsyncEnumerable<string> GetPlayerCodesByUserNamesAsync(
       [FromServices] IQueryService<GetPlayerCodesByUserNamesQuery, string> queryService,
       Guid brandId,
       [FromQuery(Name = "UserNames")] PlayerCodesQueryViewModel viewModel)
    {
        var query = new GetPlayerCodesByUserNamesQuery(
            brandId,
            viewModel.UserNames);

        return queryService
            .QueryAsync(query, HttpContext.RequestAborted);
    }

    [HttpGet("deposited/{playerCode}", Name = "GetPlayerDepositedAmount")]
    [SuccessStatusCode(200, "Player deposited amount returns")]
    public ValueTask<decimal> GetPlayerDepositedAmountAsync(
        [FromServices] IGetService<GetPlayerDepositedAmountQuery, decimal> getService,
        string playerCode)
        => getService.GetAsync(
            new GetPlayerDepositedAmountQuery(playerCode),
            HttpContext.RequestAborted);

    [HttpGet("{playerCode}", Name = "GetPlayerInfo")]
    [SuccessStatusCode(200, "Player info found")]
    [FailStatusCode(404, "Player info not found")]
    public async ValueTask<PlayerInfoViewModel> GetPlayerInfoAsync(
        [FromServices] IGetService<GetPlayerQuery, PlayerInfo?> getService,
        string playerCode)
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
                EmailAuthentication = playerInfo.EmailAuthentication,
                IsSetPayPassword = playerInfo.IsSetPayPassword,
                VipLevelId = playerInfo.VipLevelId,
                RegisteredOn = playerInfo.RegisteredOn
            };
    }

    [HttpGet("{brandId}/{email:regex(^[[\\w-\\.]]+@([[\\w-]]+\\.)+[[\\w-]]{{2,4}}$)}", Name = "GetPlayerInfoByEmail")]
    [SuccessStatusCode(200, "Player info found")]
    [FailStatusCode(404, "Player info not found")]
    public async Task<PlayerInfoViewModel> GetPlayerInfoByEmailAsync(
        [FromServices] IGetService<GetPlayerInfoByEmailQuery, PlayerInfo?> getService,
        Guid brandId,
        string email)
    {
        var playerInfo = await getService
            .GetAsync(new GetPlayerInfoByEmailQuery(brandId, email), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return playerInfo is null
            ? await this.FailAsync<PlayerInfoViewModel, object?>(404, default)
            : new PlayerInfoViewModel
            {
                PlayerCode = playerInfo.PlayerCode,
                BrandId = playerInfo.BrandId,
                UserName = playerInfo.UserName,
                IdentityAuthentication = playerInfo.IdentityAuthentication,
                PhoneAuthentication = playerInfo.PhoneAuthentication,
                EmailAuthentication = playerInfo.EmailAuthentication,
                IsSetPayPassword = playerInfo.IsSetPayPassword,
                VipLevelId = playerInfo.VipLevelId,
                RegisteredOn = playerInfo.RegisteredOn
            };
    }

    [HttpGet("{brandId}/{areaCode:regex(\\+?\\d+)}/{phone}", Name = "GetPlayerInfoByPhone")]
    [SuccessStatusCode(200, "Player info found")]
    [FailStatusCode(404, "Player info not found")]
    public async Task<PlayerInfoViewModel> GetPlayerInfoByPhoneAsync(
        [FromServices] IGetService<GetPlayerInfoByPhoneQuery, PlayerInfo?> getService,
        Guid brandId,
        string areaCode,
        string phone)
    {
        var playerInfo = await getService
            .GetAsync(
                new GetPlayerInfoByPhoneQuery(brandId, areaCode, phone),
                HttpContext.RequestAborted)
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
                EmailAuthentication = playerInfo.EmailAuthentication,
                IsSetPayPassword = playerInfo.IsSetPayPassword,
                VipLevelId = playerInfo.VipLevelId,
                RegisteredOn = playerInfo.RegisteredOn
            };
    }

    [HttpGet("{brandId}/{userName}", Name = "GetPlayerInfoByUserName")]
    public async Task<PlayerInfoViewModel> GetPlayerInfoByUserNameAsync(
        [FromServices] IGetService<GetPlayerInfoByUserNameQuery, PlayerInfo?> getService,
        Guid brandId,
        string userName)
    {
        var playerInfo = await getService
            .GetAsync(
                new GetPlayerInfoByUserNameQuery(brandId, userName),
                HttpContext.RequestAborted)
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
                EmailAuthentication = playerInfo.EmailAuthentication,
                IsSetPayPassword = playerInfo.IsSetPayPassword,
                VipLevelId = playerInfo.VipLevelId,
                RegisteredOn = playerInfo.RegisteredOn
            };
    }

    [HttpGet("prop/{playerCode}", Name = "GetPlayerProperties")]
    [SuccessStatusCode(200, "Player properties returns")]
    public async ValueTask<JsonDocument> GetPlayerPropertiesAsync(
        [FromServices] IGetService<GetPlayerPropertiesQuery, IImmutableDictionary<string, string>> getService,
        [FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
        string playerCode)
    {
        var jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;

        var properties = await getService
            .GetAsync(new GetPlayerPropertiesQuery(playerCode), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return JsonSerializer.SerializeToDocument(properties, jsonSerializerOptions);
    }

    [HttpGet("status/{playerCode}", Name = "GetPlayerStatus")]
    [SuccessStatusCode(200, "Player Status")]
    public ValueTask<PlayerStatus> GetPlayerStatusAsync(
        [FromServices] IGetService<GetPlayerStatusQuery, PlayerStatus> getService,
        string playerCode)
    {
        var query = new GetPlayerStatusQuery(playerCode);

        return getService.GetAsync(query, HttpContext.RequestAborted);
    }

    [HttpGet("{brandId:Guid}")]
    [SuccessStatusCode(200, "Players")]
    public IAsyncEnumerable<PlayerListItemViewModel> QueryPlayerListAsync(
        [FromServices] IQueryService<PlayerListQuery, PlayerListItem> queryService,
        Guid brandId,
        [FromQuery] PlayerListQueryViewModel queryViewModel,
        [FromQuery] PagingViewModel pagingModel)
    {
        var query = new PlayerListQuery(
            brandId,
            queryViewModel.PlayerCodes,
            queryViewModel.Status,
            queryViewModel.VipId,
            queryViewModel.UserName,
            queryViewModel.AgentId,
            queryViewModel.ParentPlayerCode,
            queryViewModel.Phone,
            queryViewModel.RealName,
            queryViewModel.RegisteredDomainName,
            queryViewModel.BeginRegisteredTime,
            queryViewModel.EndRegisteredTime);

        var paging = new Paging<PlayerListItem>();

        if (!string.IsNullOrEmpty(pagingModel.SortBy))
            paging.SortByPropertyName(pagingModel.SortBy, pagingModel.IsDesc ?? false);

        paging.RowCount = pagingModel.Rows ?? 1000;
        paging.Page = pagingModel.Page ?? 1;

        var result = queryService.QueryAsync(query, HttpContext.RequestAborted);

        var totalCount = result.CountAsync(HttpContext.RequestAborted);

        var responseFeature = HttpContext.Features.Get<IHttpResponseFeature>();
        if (responseFeature is not null)
            _ = responseFeature.Headers.TryAdd("TotalCount", totalCount.ToString());

        return result
            .ApplyOrderBy(paging)
            .ApplyPaging(paging)
            .Select(item => new PlayerListItemViewModel
            {
                PlayerCode = item.PlayerCode,
                AgentId = item.AgentId,
                ParentPlayerCode = item.ParentPlayerCode,
                RealName = item.RealName,
                RegisteredDomainName = item.RegisteredDomainName,
                RegisteredOn = item.RegisteredOn,
                Status = item.Status,
                UserName = item.UserName,
                VipId = item.VipId
            });
    }

    [HttpPost("prop/{playerCode}", Name = "SetPlayerProperties")]
    [SuccessStatusCode(200, "Set player properties success")]
    [FailStatusCode(400, "Input properties is empty")]
    public ValueTask SetPlayerPropertiesAsync(
        [FromServices] ICommandService<SetPlayerPropertiesCommand> commandService,
        string playerCode,
        [FromBody] IImmutableDictionary<string, string?> properties)
    {
        if (properties.Count == 0)
            return new ValueTask(this.FailAsync(400, "Input properties is empty"));

        var command = new SetPlayerPropertiesCommand(
            playerCode,
            properties);

        return commandService
            .ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpPut("status/{playerCode}", Name = "SetPlayerStatus")]
    [SuccessStatusCode(200, "Status set")]
    public ValueTask SetPlayerStatusAsync(
        [FromServices] ICommandService<SetPlayerStatusCommand> setService,
        string playerCode,
        [FromBody] PlayerStatus status)
    {
        var command = new SetPlayerStatusCommand(playerCode, status);

        return setService.ExecuteAsync(command, HttpContext.RequestAborted);
    }
}
