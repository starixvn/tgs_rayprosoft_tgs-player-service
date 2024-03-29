namespace Tgs.PlayerService.Client;

public record QueryPlayerListConditions(
    Guid? AgentId,
    DateTime? BeginRegisteredTime,
    DateTime? EndRegisteredTime,
    string? ParentPlayerCode,
    string? Phone,
    IEnumerable<string> PlayerCodes,
    string? RealName,
    PlayerStatus? Status,
    string? UserName,
    Guid? VipId)
{
    public static readonly QueryPlayerListConditions Empty = new(
        null,
        null,
        null,
        null,
        null,
        Array.Empty<string>(),
        null,
        null,
        null,
        null);
}
