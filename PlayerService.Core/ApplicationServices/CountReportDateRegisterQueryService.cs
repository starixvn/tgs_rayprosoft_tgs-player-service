using Tgs.MarketingReport.Client;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.PlayerService.ApplicationServices;

internal class CountReportDateRegisterQueryService(
    ISystemClock systemClock,
    IPlayerRepository playerRepository,
    IMarketingReportClient marketingReportClient) : IGetService<CountReportDateRegisterQuery, int>
{
    private readonly IMarketingReportClient _marketingReportClient = marketingReportClient ?? throw new ArgumentNullException(nameof(marketingReportClient));
    private readonly ISystemClock _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));

    public async ValueTask<int> GetAsync(CountReportDateRegisterQuery query, CancellationToken cancellationToken = default)
    {
        var now = _systemClock.UtcNow.ToOffset(TimeSpan.FromHours(8D));
        var currentDate = DateOnly.FromDateTime(now.DateTime);
        var yesterday = currentDate.AddDays(-1);

        var scheduleTime = new DateTimeOffset(now.Year, now.Month, now.Day, 8, 0, 0, TimeSpan.FromHours(8));

        if (query.ReportDate == currentDate)
        {
            var currenyDayRegisterCount = await playerRepository
                .CurrentDayRegisterAsync(query.BrandId, cancellationToken)
                .ConfigureAwait(false);

            return currenyDayRegisterCount;
        }

        if (query.ReportDate == yesterday && now < scheduleTime)
        {
            var yesterDayRegisterCount = await playerRepository
                .CountYesterdayReigsterAsync(query.BrandId, cancellationToken)
                .ConfigureAwait(false);

            return yesterDayRegisterCount;
        }

        var reportData = await _marketingReportClient
            .QueryBrandMarketingStatisticsAsync(query.BrandId, query.ReportDate, cancellationToken)
            .ConfigureAwait(false);

        return reportData?.TotalRegisterCount ?? 0;
    }
}
