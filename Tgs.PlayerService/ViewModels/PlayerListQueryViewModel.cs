using Microsoft.AspNetCore.Mvc;

namespace Tgs.PlayerService.ViewModels;

public class PlayerListQueryViewModel
{
    [FromQuery(Name = "PlayerCode")]
    public string[] PlayerCodes { get; set; } = Array.Empty<string>();

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

    [FromQuery]
    public string? RealName { get; set; }

    [FromQuery]
    public string? RegisteredDomainName { get; set; }

    [FromQuery]
    public PlayerStatus? Status { get; set; }

    [FromQuery]
    public string? UserName { get; set; }

    [FromQuery]
    public Guid? VipId { get; set; }

    [FromQuery]
    public PlayerTagType? TagType { get; set; }
}
