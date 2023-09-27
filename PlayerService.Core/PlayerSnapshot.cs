namespace Tgs.PlayerService;

public record PlayerSnapshot(
    string PlayerCode,
    Guid BrandId,
    Guid? AgentId,
    Guid VipId,
    string? IdNumber,
    string Username,
    string? RealName,
    PlayerType Type,
    PlayerStatus PlayerStatus,
    string? Email,
    string? AreaCode,
    string? Phone,
    string? Qq,
    string? Wechat,
    bool IdentityAuthentication,
    bool PhoneAuthentication,
    DateOnly? Birthdate,
    string? UpperPlayerCode,
    string? Remark)
{
    internal static readonly PlayerSnapshot _Default = new(
        PlayerCode: string.Empty,
        BrandId: Guid.Empty,
        AgentId: null,
        VipId: Guid.Empty,
        IdNumber: null,
        Username: string.Empty,
        RealName: null,
        Type: PlayerType.None,
        PlayerStatus.Disabled,
        Email: null,
        AreaCode: null,
        Phone: null,
        Qq: null,
        Wechat: null,
        IdentityAuthentication: false,
        PhoneAuthentication: false,
        Birthdate: null,
        UpperPlayerCode: null,
        Remark: null);
}
