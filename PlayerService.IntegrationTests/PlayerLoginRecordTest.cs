using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.PlayerService.Client;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.IntegrationTests;

[TestFixture]
public class PlayerLoginRecordTest
{
    [Test]
    public async Task Query_Player_Login_Record_Group_Player()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var startOn = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var endOn = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var page = 1;
        var rows = 2;

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.PlayerLoginRecords.AddAsync(new PlayerLoginRecord
        {
            Id = Guid.NewGuid(),
            PlayerCode = "test",
            BrandId = brandId,
            Status = 0,
            PlatformType = 1,
            IpAddress = "127.0.0.1",
            CreatedOn = new DateTimeOffset(2024, 2, 5, 17, 24, 33, TimeSpan.Zero).DateTime,
            ExpiredOn = new DateTimeOffset(2024, 2, 6, 17, 24, 33, TimeSpan.Zero).DateTime
        });
        _ = await dbContext.PlayerLoginRecords.AddAsync(new PlayerLoginRecord
        {
            Id = Guid.NewGuid(),
            PlayerCode = Guid.NewGuid().ToString("N"),
            BrandId = Guid.NewGuid(),
            Status = 0,
            PlatformType = 1,
            IpAddress = "127.0.0.1",
            CreatedOn = new DateTimeOffset(2024, 2, 5, 17, 24, 33, TimeSpan.Zero).DateTime,
            ExpiredOn = new DateTimeOffset(2024, 2, 6, 17, 24, 33, TimeSpan.Zero).DateTime
        });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.QueryPlayerLoginRecordGroupPlayerAsync(brandId, startOn, endOn, page, rows).ToArrayAsync();

        ClassicAssert.AreEqual("test", actual.FirstOrDefault());
    }
}
