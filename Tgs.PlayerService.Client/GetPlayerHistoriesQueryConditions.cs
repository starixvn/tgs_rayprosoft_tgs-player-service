namespace Tgs.PlayerService.Client;

public class GetPlayerHistoriesQueryConditions
{
    public DateTimeOffset From { get; set; }

    public string PlayerCode { get; set; } = default!;

    public DateTimeOffset To { get; set; }
}
