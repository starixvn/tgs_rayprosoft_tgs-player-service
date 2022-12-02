namespace Tgs.PlayerService.Client;

public class PlayerListQuery
{
    public Guid? AgentId { get; init; }

    public DateTime? BeginRegisteredTime { get; init; }

    public DateTime? EndRegisteredTime { get; init; }

    public string? ParentPlayerCode { get; init; }

    public string? Phone { get; init; }

    public IEnumerable<string> PlayerCodes { get; init; } = Array.Empty<string>();

    public string? RealName { get; init; }

    public string? RegisteredDomainName { get; init; }

    public PlayerStatus? Status { get; init; }

    public string? UserName { get; init; }

    public Guid? VipId { get; init; }

    public PlayerTagType? TagType { get; set; }
}
