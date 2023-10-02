namespace Tgs.PlayerService.Client;

public class PagingResponse<TItem>
{
    public IEnumerable<TItem> Items { get; set; } = default!;

    public int Page { get; set; }

    public int Rows { get; set; }

    public int TotalCount { get; set; }
}
