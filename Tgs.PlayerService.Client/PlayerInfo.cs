using System.Text.Json.Serialization;

namespace Tgs.PlayerService.Client;

public record PlayerInfo
{
    public Guid BrandId { get; init; }

    public bool IdentityAuthentication { get; init; }

    public bool IsSetPayPassword { get; init; }

    public bool PhoneAuthentication { get; init; }

    public bool EmailAuthentication { get; init; }

    public string PlayerCode { get; init; } = default!;

    [JsonPropertyName("username")]
    public string UserName { get; init; } = default!;

    public Guid? VipLevelId { get; init; }

    public DateTime? RegisteredOn { get; init; }

    public Guid? AgentId { get; init; }
}
