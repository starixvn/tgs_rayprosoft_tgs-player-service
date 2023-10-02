using Tgs.Paging;

namespace Tgs.PlayerService.ApplicationServices;

public record GetPlayerHistoriesQuery(
    string PlayerCode,
    DateTimeOffset From,
    DateTimeOffset To,
    Paging<PlayerHistoryInfo> Paging);
