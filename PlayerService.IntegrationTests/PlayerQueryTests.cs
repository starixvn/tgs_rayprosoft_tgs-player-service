using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tgs.PlayerService.Client;
using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.IntegrationTests;

public class PlayerQueryTests
{
    [Test]
    public async Task PlayerListQuery_AgentId_Can_Parse()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var query = new PlayerListQuery();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var agentId = Guid.NewGuid();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                AgentId = agentId,
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(agentId, actual.First().AgentId);
    }

    [Test]
    public async Task PlayerListQuery_By_BrandId()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var query = new PlayerListQuery();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = Guid.NewGuid(),
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(2, actual.Length);
    }

    [Test]
    public async Task PlayerListQuery_By_Players()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var query = new PlayerListQuery
        {
            PlayerCodes = new[] { "player1", "player2" }
        };

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

        _ = await dbContext.SaveChangesAsync();

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(2, actual.Length);
    }

    [Test]
    public async Task PlayerListQuery_By_UserName()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = Guid.NewGuid(),
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

        _ = await dbContext.SaveChangesAsync();

        var query = new PlayerListQuery
        {
            UserName = "player2"
        };

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
    }

    [Test]
    public async Task QueryPlayer_List_By_Status()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                Status = (short)Client.PlayerStatus.RegisterUndone
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-2")
            });

        _ = await dbContext.SaveChangesAsync();

        var query = new PlayerListQuery
        {
            Status = Client.PlayerStatus.RegisterUndone
        };

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
    }

    [Test]
    public async Task QueryPlayerList_By_AgentId()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                AgentId = agentId
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = Guid.NewGuid(),
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

        _ = await dbContext.SaveChangesAsync();

        var query = new PlayerListQuery
        {
            AgentId = agentId
        };

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
    }

    [Test]
    public async Task QueryPlayerList_By_ParentPlayerCode()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var parentPlayerCode = "parent";

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                Phone = "123456"
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-2")
            });

        _ = await dbContext.PlayerParents.AddAsync(new PlayerParent
        {
            PlayerCode = "player3",
            ParentPlayerCode = parentPlayerCode
        });

        _ = await dbContext.SaveChangesAsync();

        var query = new PlayerListQuery
        {
            ParentPlayerCode = parentPlayerCode
        };

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
    }

    [Test]
    public async Task QueryPlayerList_By_Phone()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                Phone = "123456"
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-2")
            });

        _ = await dbContext.SaveChangesAsync();

        var query = new PlayerListQuery
        {
            Phone = "123456"
        };

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
    }

    [Test]
    public async Task QueryPlayerList_By_RealName()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RealName = "real name"
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                Phone = "123456"
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-2")
            });

        _ = await dbContext.SaveChangesAsync();

        var query = new PlayerListQuery
        {
            RealName = "real name"
        };

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
    }

    [Test]
    public async Task QueryPlayerList_By_RegisteredDomainName()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                Phone = "123456"
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-2")
            });

        _ = await dbContext.PlayerIpRecords.AddAsync(new PlayerIpRecord
        {
            PlayerCode = "player3",
            Host = "test.com",
            Ip = "127.0.0.1",
            IpType = 1
        });

        _ = await dbContext.SaveChangesAsync();

        var query = new PlayerListQuery
        {
            RegisteredDomainName = "test.com"
        };

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
    }

    [Test]
    public async Task QueryPlayerList_By_RegisteredOn()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-4")
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-1")
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-2")
            },
            new Player
            {
                PlayerCode = "player4",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player4",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-3")
            },
            new Player
            {
                PlayerCode = "player5",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player5",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

        _ = await dbContext.SaveChangesAsync();

        var query = new PlayerListQuery
        {
            BeginRegisteredTime = DateTime.Parse("2022-7-1"),
            EndRegisteredTime = DateTime.Parse("2022-7-4")
        };

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(3, actual.Length);
    }

    [Test]
    public async Task QueryPlayerList_By_VipId()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();
        var vipId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                Phone = "123456"
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "player3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-2"),
                VipId = vipId
            });

        _ = await dbContext.SaveChangesAsync();

        var query = new PlayerListQuery
        {
            VipId = vipId
        };

        var actual = await sut.QueryPlayerListAsync(brandId, query).ToArrayAsync();

        Assert.AreEqual(1, actual.Length);
    }

    [Test]
    public async Task 用UserNames查詢PlayerCodes()
    {
        await using var web = new TestPlayerServiceApplication();

        var httpClient = web.CreateClient();

        var sut = new PlayerServiceClient(httpClient);

        var brandId = Guid.NewGuid();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        await dbContext.Players.AddRangeAsync(
            new Player
            {
                PlayerCode = "player1",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "userName1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
            },
            new Player
            {
                PlayerCode = "player2",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "userName2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                Phone = "123456"
            },
            new Player
            {
                PlayerCode = "player3",
                BrandId = brandId,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "userName3",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                RegisteredOn = DateTime.Parse("2022-7-2")
            });

        _ = await dbContext.SaveChangesAsync();

        var userNames = new[] { "userName1", "userName2" };

        var expected = new[] { "player1", "player2" };

        var actual = await sut.GetPlayerCodesByUserNamesAsync(brandId, userNames)
            .ToArrayAsync();

        Assert.AreEqual(expected, actual);
    }
}
