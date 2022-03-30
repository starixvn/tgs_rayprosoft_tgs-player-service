using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using Tgs.PlayerService.Core;
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
    public async Task Can_Get_Player_Info()
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
            IdentityAuthentication = true,
            PhoneAuthentication = false,
            Status = (short)PlayerStatus.Enabled,
            VipId = Guid.NewGuid(),
            UpdatedOn = DateTime.Now,
            UpdatedByUsername = string.Empty,
        };

        _ = await context.Players.AddAsync(player).ConfigureAwait(false);
        _ = await context.SaveChangesAsync().ConfigureAwait(false);

        var expected = new PlayerInfo(
            player.PlayerCode,
            player.BrandId,
            player.Username,
            player.IdentityAuthentication,
            player.PhoneAuthentication,
            (PlayerStatus)player.Status,
            player.WithdrawPassword.Length > 0,
            player.VipId);

        var sut = new PlayerRepository(context);

        var actual = await sut
            .GetPlayerInfoAsync(playerCode)
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

    [Test]
    public async Task Cannot_Get_PlayerInfo_When_PlayerCode_Not_Exist()
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
            IdentityAuthentication = true,
            PhoneAuthentication = false,
            Status = (short)PlayerStatus.Enabled,
            VipId = Guid.NewGuid(),
            UpdatedOn = DateTime.Now,
            UpdatedByUsername = string.Empty,
        };

        _ = await context.Players.AddAsync(player).ConfigureAwait(false);
        _ = await context.SaveChangesAsync().ConfigureAwait(false);

        var sut = new PlayerRepository(context);

        var actual = await sut
            .GetPlayerInfoAsync("4ucmqplxb030")
            .ConfigureAwait(false);

        Assert.IsNull(actual);
    }
}
