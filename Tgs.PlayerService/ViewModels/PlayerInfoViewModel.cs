using System.Text.Json.Serialization;

namespace Tgs.PlayerService.ViewModels;

public class PlayerInfoViewModel
{
    public required string PlayerCode { get; set; }

    public required Guid BrandId { get; set; }

    [JsonPropertyName("username")]
    public required string UserName { get; set; }

    public required bool EmailAuthentication { get; set; }

    public required bool IdentityAuthentication { get; set; }

    public required bool IsSetPayPassword { get; set; }

    public required bool PhoneAuthentication { get; set; }

    public DateTime? RegisteredOn { get; set; }

    public Guid? VipLevelId { get; set; }

    public required PlayerStatus Status { get; set; }
}
