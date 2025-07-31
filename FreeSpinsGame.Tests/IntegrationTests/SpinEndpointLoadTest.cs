using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace FreeSpinsGame.Tests.IntegrationTests;

public class SpinEndpointLoadTest
{
    private readonly HttpClient _client;
    private readonly string _connectionString;
    
    public SpinEndpointLoadTest()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var settings = config.GetSection("TestSettings").Get<TestSettings>();
        
        _client = new HttpClient { BaseAddress = new Uri(settings!.BaseAddress) };
        _connectionString = settings.DbConnectionString;
    }

    [Fact, Trait("Category", "Integration")]
    public async Task SpinEndpoint_ShouldAllowFiveSpins_AndReturnForbiddenAfterLimit_WhenBombarded()
    {
        const int parallelRequests = 50;
        const int maxAllowedSpinCount = 5;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var tasks = new Task<HttpResponseMessage>[parallelRequests];
        
        var campaignId = await connection
            .QueryFirstAsync<int>("INSERT INTO Campaigns DEFAULT VALUES; SELECT SCOPE_IDENTITY();");

        var playerId = await connection
            .QueryFirstAsync<int>("INSERT INTO Players DEFAULT VALUES; SELECT SCOPE_IDENTITY();");

        for (var i = 0; i < parallelRequests; i++)
        {
            tasks[i] = _client.PostAsync($"/campaigns/{campaignId}/players/{playerId}/spin", null);
        }

        await Task.WhenAll(tasks);

        await connection.ExecuteAsync(
            """
            DELETE FROM Players
            WHERE Id = @playerId;

            DBCC CHECKIDENT ('Players', RESEED, @playerReseedValue);

            DELETE FROM Campaigns
            WHERE Id = @campaignId;

            DBCC CHECKIDENT ('Campaigns', RESEED, @campaignReseedValue);
            """,
            new
            {
                playerId,
                campaignId,
                playerReseedValue = playerId - 1,
                campaignReseedValue = campaignId - 1
            });

        var successCount = 0;
        var forbiddenCount = 0;

        foreach (var task in tasks)
        {
            var response = await task;

            if (response.IsSuccessStatusCode)
            {
                successCount++;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                forbiddenCount++;
            }
        }
        
        Assert.Equal(maxAllowedSpinCount, successCount);
        Assert.Equal(parallelRequests - maxAllowedSpinCount, forbiddenCount);
    }
}