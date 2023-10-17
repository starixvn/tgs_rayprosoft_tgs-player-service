using Microsoft.AspNetCore.Mvc;

namespace Tgs.PlayerService.ViewModels;

public class CheckSameEmailQueryViewModel
{
    [FromQuery(Name = "brandId")]
    public Guid BrandId { get; set; }

    [FromQuery(Name = "email")]
    public string Email { get; set; } = default!;
}
