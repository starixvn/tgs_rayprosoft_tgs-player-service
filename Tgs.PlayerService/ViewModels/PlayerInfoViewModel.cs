using System.Text.Json.Serialization;

namespace Tgs.PlayerService.ViewModels;

public class PlayerInfoViewModel
{
    public Guid BrandId { get; init; }

    public bool IdentityAuthentication { get; init; }

    public bool IsSetPayPassword { get; init; }

    public bool PhoneAuthentication { get; init; }

    public string PlayerCode { get; init; } = default!;

    [JsonPropertyName("username")]
    public string UserName { get; init; } = default!;

    public Guid? VipLevelId { get; init; }

    public DateTime? RegisteredOn { get; set; }
}
