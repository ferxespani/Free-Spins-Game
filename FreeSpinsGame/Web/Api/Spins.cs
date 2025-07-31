using FreeSpinsGame.Application.Common.Interfaces;
using FreeSpinsGame.Domain.Enums;
using FreeSpinsGame.Web.Autoregistration;

namespace FreeSpinsGame.Web.Api;

public class Spins : IApiRoute
{
    public void Register(WebApplication group)
    {
        group.MapPost("/campaigns/{campaignId:int}/players/{playerId:int}/spin", Spin);
        
        group.MapGet("/campaigns/{campaignId:int}/players/{playerId:int}", GetSpinData);

        group.MapGet("/history", GetSpinHistory);
    }
    
    private static async Task<IResult> Spin(
        int campaignId,
        int playerId,
        ISpinService spinService)
    {
        var spinResult = await spinService.SpinAsync(campaignId, playerId);

        if (spinResult.Status == SpinStatus.Locked)
        {
            var isAllowedOrLimit = false;

            while (!isAllowedOrLimit)
            {
                spinResult = await spinService.SpinAsync(campaignId, playerId);

                isAllowedOrLimit = spinResult.Status is SpinStatus.Allowed or SpinStatus.LimitReached;
            }
        }

        return spinResult.Status == SpinStatus.Allowed
            ? Results.Ok(spinResult.CurrentCount)
            : Results.StatusCode(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> GetSpinData(
        int campaignId,
        int playerId,
        ISpinService spinService)
    {
        var spinData = await spinService.GetSpinDataAsync(campaignId, playerId);

        return spinData is not null
            ? Results.Ok(spinData)
            : Results.NotFound();
    }

    private static async Task<IResult> GetSpinHistory(
        int? campaignId,
        int? playerId,
        ISpinService spinService)
    {
        return Results.Ok(await spinService.GetSpinHistoryAsync(campaignId, playerId));
    }
}