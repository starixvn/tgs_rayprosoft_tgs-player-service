namespace Tgs.PlayerService.ViewModels;

public class PlayerListItemViewModel
{
    public Guid? AgentId { get; set; }

    public string? ParentPlayerCode { get; set; }

    public string PlayerCode { get; set; } = default!;

    public string? RealName { get; set; }

    public string? RegisteredDomainName { get; set; }

    public DateTime? RegisteredOn { get; set; }

    public PlayerStatus Status { get; set; }

    public string UserName { get; set; } = default!;
}
