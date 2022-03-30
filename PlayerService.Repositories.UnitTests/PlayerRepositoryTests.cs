using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.Repositories.UnitTests;

[TestFixture]
public class PlayerRepositoryTests
{
    [Test]
    public async Task Can_Get_Player_Deposited_Amount()
    {
        var options = new DbContextOptionsBuilder<TgsContext>()
            .UseInMemoryDatabase(databaseName: "dbo")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new TgsContext(options);
        _ = await context.Database.EnsureDeletedAsync();

        var playerCode = "4ucmqplxb029";
        var expected = 2000m;

        var player = new Player
        {
            PlayerCode = playerCode,
            Username = playerCode,
            BrandId = Guid.NewGuid(),
            RealName = playerCode,
            Password = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
            WithdrawPassword = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
            WithdrawSalt = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
            Salt = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
            DefaultWalletCurrencyCode = "CNY",
            DefaultWalletId = Guid.NewGuid(),
            AccumulatedDeposit = expected,
            UpdatedOn = DateTime.Now,
            UpdatedByUsername = string.Empty,
        };

        _ = await context.Players.AddAsync(player).ConfigureAwait(false);
        _ = await context.SaveChangesAsync().ConfigureAwait(false);

        var sut = new PlayerRepository(context);

        var actual = await sut
            .GetPlayerDepositedAmountAsync(playerCode)
            .ConfigureAwait(false);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task Cannot_Get_Player_Deposited_Amount_When_PlayerCode_Not_Exist()
    {
        var options = new DbContextOptionsBuilder<TgsContext>()
            .UseInMemoryDatabase(databaseName: "dbo")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new TgsContext(options);
        _ = await context.Database.EnsureDeletedAsync();

        var playerCode = "4ucmqplxb029";

        var player = new Player
        {
            PlayerCode = playerCode,
            Username = playerCode,
            BrandId = Guid.NewGuid(),
            RealName = playerCode,
            Password = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
            WithdrawPassword = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
            WithdrawSalt = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
            Salt = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
            DefaultWalletCurrencyCode = "CNY",
            DefaultWalletId = Guid.NewGuid(),
            AccumulatedDeposit = 2000m,
            UpdatedOn = DateTime.Now,
            UpdatedByUsername = string.Empty,
        };

        _ = await context.Players.AddAsync(player).ConfigureAwait(false);
        _ = await context.SaveChangesAsync().ConfigureAwait(false);

        var sut = new PlayerRepository(context);

        var actual = await sut
            .GetPlayerDepositedAmountAsync("4ucmqplxb030")
            .ConfigureAwait(false);

        Assert.IsNull(actual);
    }
}
