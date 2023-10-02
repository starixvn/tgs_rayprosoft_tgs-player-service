using Microsoft.AspNetCore.Mvc;

namespace Tgs.PlayerService.ViewModels;

public class PagingViewModel
{
    [FromQuery(Name = "desc")]
    public bool? IsDesc { get; set; }

    [FromQuery(Name = "page")]
    public int? Page { get; set; }

    [FromQuery(Name = "rows")]
    public int? Rows { get; set; }

    [FromQuery(Name = "sort")]
    public string? SortBy { get; set; }
}
