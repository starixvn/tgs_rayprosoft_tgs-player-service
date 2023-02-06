using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tgs.PlayerService.Client;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.IntegrationTests;

public class PlayerDepositTests
{
    [Test]
    public async Task 加總玩家總存款()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var playerCode = "test1";
        var amount = 1000M;

        var playerRow = new Player
        {
            PlayerCode = playerCode,
            Username = "tester",
            AccumulatedDeposit = 0M,
            DefaultWalletCurrencyCode = "CNY",
            Password = Array.Empty<byte>(),
            Salt = Array.Empty<byte>(),
            WithdrawPassword = Array.Empty<byte>(),
            WithdrawSalt = Array.Empty<byte>()
        };

        _ = await dbContext.Players.AddAsync(playerRow);

        _ = await dbContext.SaveChangesAsync();

        await sut.TopUpAccumulatedDepositAsync(playerCode, amount);

        Assert.AreEqual(amount, playerRow.AccumulatedDeposit);
    }
}
