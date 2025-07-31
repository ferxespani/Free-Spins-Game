using FreeSpinsGame.Application.Common.Interfaces;
using FreeSpinsGame.Application.Services;
using FreeSpinsGame.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace FreeSpinsGame;

public static class ProgramExtensions
{
    public static void RegisterApplicationsServices(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddDbContext<SpinGameDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        var redis = ConnectionMultiplexer.Connect(configuration["Redis:Host"]!);
        var multiplexers = new List<RedLockMultiplexer>
        {
            redis
        };

        var redLockFactory = RedLockFactory.Create(multiplexers);

        services.AddSingleton<IConnectionMultiplexer>(redis);
        services.AddSingleton<IDistributedLockFactory>(redLockFactory);

        services.AddScoped<ISpinService, SpinService>();
    }
}