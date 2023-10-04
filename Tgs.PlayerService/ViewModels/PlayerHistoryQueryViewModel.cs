using Microsoft.AspNetCore.Mvc;

namespace Tgs.PlayerService.ViewModels;

public class PlayerHistoryQueryViewModel
{
    [FromQuery(Name = "from")]
    public DateTimeOffset From { get; set; }

    [FromQuery(Name = "to")]
    public DateTimeOffset To { get; set; }
}
