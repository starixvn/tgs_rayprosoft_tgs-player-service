namespace Tgs.PlayerService.ViewModels;

public class PagingResponseViewModel<TItem>
{
    public IEnumerable<TItem> Items { get; set; } = default!;

    public int Page { get; set; }

    public int Rows { get; set; }

    public int TotalCount { get; set; }
}
