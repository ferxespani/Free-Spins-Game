using FreeSpinsGame.Application.Services;
using FreeSpinsGame.Domain.Entities;
using FreeSpinsGame.Domain.Enums;
using FreeSpinsGame.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RedLockNet;

namespace FreeSpinsGame.Tests.UnitTests;

public class SpinServiceTests
{
    private readonly Mock<IDistributedLockFactory> _lockFactoryMock = new();
    private readonly Mock<IRedLock> _lockMock = new();
    private readonly Mock<ILogger<SpinService>> _loggerMock = new();

    private SpinService CreateService(SpinGameDbContext context)
    {
        _lockMock
            .SetupGet(x => x.IsAcquired)
            .Returns(true);
        
        _lockFactoryMock
            .Setup(x => x.CreateLockAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>()))
            .ReturnsAsync(_lockMock.Object);

        return new SpinService(_lockFactoryMock.Object, context, _loggerMock.Object);
    }

    private static SpinGameDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<SpinGameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        return new SpinGameDbContext(options);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task LockNotAcquired_ReturnsLockFailed()
    {
        _lockMock
            .SetupGet(x => x.IsAcquired)
            .Returns(false);
        
        _lockFactoryMock
            .Setup(x => x.CreateLockAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>()))
            .ReturnsAsync(_lockMock.Object);

        await using var context = CreateInMemoryDbContext();
        
        var service = new SpinService(_lockFactoryMock.Object, context, _loggerMock.Object);

        var result = await service.SpinAsync(1, 1);

        Assert.Equal(SpinStatus.Locked, result.Status);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task SpinLimitReached_ReturnsLimitReached()
    {
        await using var context = CreateInMemoryDbContext();
        
        var campaign = new Campaign { MaxSpinCount = 2 };
        var player = new Player();
        
        var playerCampaign = new PlayerCampaign
        {
            CurrentSpinCount = 2,
            Campaign = campaign,
            Player = player
        };

        context.Campaigns.Add(campaign);
        context.Players.Add(player);
        context.PlayerCampaigns.Add(playerCampaign);
        
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SpinAsync(1, 1);

        Assert.Equal(SpinStatus.LimitReached, result.Status);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task ExistingPlayerCampaign_IncrementsCount_AndReturnsAllowed()
    {
        await using var context = CreateInMemoryDbContext();
        
        var campaign = new Campaign { MaxSpinCount = 5 };
        var player = new Player();
        
        var playerCampaign = new PlayerCampaign
        {
            CurrentSpinCount = 2,
            Campaign = campaign,
            Player = player
        };

        context.Campaigns.Add(campaign);
        context.Players.Add(player);
        context.PlayerCampaigns.Add(playerCampaign);
        
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SpinAsync(1, 1);

        Assert.Equal(SpinStatus.Allowed, result.Status);
        Assert.Equal(3, result.CurrentCount);
    }
}