using Microsoft.AspNetCore.Mvc;
using Tgs.PlayerService.ModelBinders;

namespace Tgs.PlayerService.ViewModels;

public class PlayerQueryCoditionViewModel
{
    [FromQuery]
    public Guid? AgentId { get; set; }

    [FromQuery]
    public DateTime? BeginRegisteredTime { get; set; }

    [FromQuery]
    public DateTime? EndRegisteredTime { get; set; }

    [FromQuery]
    public string? ParentPlayerCode { get; set; }

    [FromQuery]
    public string? Phone { get; set; }

    [ModelBinder(BinderType = typeof(CommaSeparatedStringsModelBinder<string>))]
    public string[] PlayerCodes { get; set; } = [];

    [FromQuery]
    public string? RealName { get; set; }

    [FromQuery]
    public PlayerStatus? Status { get; set; }

    [FromQuery]
    public string? UserName { get; set; }

    [FromQuery]
    public Guid? VipId { get; set; }
}
