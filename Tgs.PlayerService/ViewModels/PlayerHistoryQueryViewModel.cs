using Microsoft.AspNetCore.Mvc;

namespace Tgs.PlayerService.ViewModels;

public class PlayerHistoryQueryViewModel
{
    [FromQuery(Name = "from")]
    public DateTime From { get; set; }

    [FromQuery(Name = "to")]
    public DateTime To { get; set; }
}
