namespace Tgs.PlayerService.ViewModels;

public class PlayerListQueryViewModel
{
    public Guid? AgentId { get; set; }

    public DateTime? BeginRegisteredTime { get; set; }

    public DateTime? EndRegisteredTime { get; set; }

    public string? ParentPlayerCode { get; set; }

    public string? Phone { get; set; }

    public string? RealName { get; set; }

    public string? RegisteredDomainName { get; set; }

    public PlayerStatus? Status { get; set; }

    public string? UserName { get; set; }

    public Guid? VipId { get; set; }
}
