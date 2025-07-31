using FreeSpinsGame.Domain.Entities;

namespace FreeSpinsGame.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(SpinGameDbContext dbContext)
    {
        if (!dbContext.Players.Any())
        {
            dbContext.AddRange(new List<Player> { new(), new() });
        }

        if (!dbContext.Campaigns.Any())
        {
            dbContext.AddRange(new List<Campaign> { new(), new() });
        }
        
        await dbContext.SaveChangesAsync();
    }
}