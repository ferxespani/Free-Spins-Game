using FreeSpinsGame.Application.Common.Interfaces;
using FreeSpinsGame.Domain.Entities;
using FreeSpinsGame.Domain.Enums;
using FreeSpinsGame.Infrastructure.Data;
using FreeSpinsGame.Web.Dtos;
using Microsoft.EntityFrameworkCore;
using RedLockNet;

namespace FreeSpinsGame.Application.Services;

public class SpinService(
    IDistributedLockFactory lockFactory,
    SpinGameDbContext dbContext,
    ILogger<SpinService> logger) : ISpinService
{
    public async Task<SpinResultDto> SpinAsync(int campaignId, int playerId)
    {
        var lockKey = $"lock:spin:{campaignId}:{playerId}";
        var expiry = TimeSpan.FromSeconds(5);

        await using var redLock = await lockFactory.CreateLockAsync(lockKey, expiry);

        if (!redLock.IsAcquired)
        {
            logger.LogInformation(
                "Lock failed for player: {PlayerId} and campaign: {CampaignId}",
                playerId,
                campaignId);
            
            return SpinResultDto.LockFailed;
        }

        var playerCampaign = await dbContext.PlayerCampaigns
            .Include(x => x.Campaign)
            .FirstOrDefaultAsync(x => x.CampaignId == campaignId && x.PlayerId == playerId);

        if (playerCampaign is null)
        {
            dbContext.PlayerCampaigns.Add(new PlayerCampaign
            {
                CampaignId = campaignId,
                PlayerId = playerId,
                CurrentSpinCount = 1
            });
        }
        else if (playerCampaign.CurrentSpinCount >= playerCampaign.Campaign.MaxSpinCount)
        {
            logger.LogInformation(
                "Max spin count: {SpinCount} reached for player: {PlayerId} and campaign: {CampaignId}",
                playerCampaign.Campaign.MaxSpinCount,
                playerId,
                campaignId);
            
            return SpinResultDto.LimitReached;
        }
        else
        {
            playerCampaign.CurrentSpinCount++;
        }

        dbContext.SpinHistory.Add(new SpinHistory
        {
            PlayerId = playerId,
            CampaignId = campaignId,
            Date = DateTime.UtcNow
        });

        logger.LogInformation("Spin is successful for player: {PlayerId} and campaign: {CampaignId}",
            playerId,
            campaignId);

        await dbContext.SaveChangesAsync();

        return new SpinResultDto
        {
            Status = SpinStatus.Allowed,
            CurrentCount = playerCampaign?.CurrentSpinCount ?? 1
        };
    }

    public async Task<SpinDataDto?> GetSpinDataAsync(int campaignId, int playerId)
    {
        var playerCampaign = await dbContext.PlayerCampaigns
            .Include(x => x.Campaign)
            .FirstOrDefaultAsync(x => x.CampaignId == campaignId && x.PlayerId == playerId);

        if (playerCampaign is null)
        {
            return null;
        }

        return new SpinDataDto
        {
            CurrentSpinUsage = playerCampaign.CurrentSpinCount,
            MaxAllowedSpins = playerCampaign.Campaign.MaxSpinCount
        };
    }

    public async Task<List<SpinHistoryDto>> GetSpinHistoryAsync(int? campaignId, int? playerId)
    {
        var spinHistoryQuery = dbContext.SpinHistory.AsQueryable();

        if (campaignId is not null && playerId is not null)
        {
            spinHistoryQuery = spinHistoryQuery
                .Where(x => x.CampaignId == campaignId && x.PlayerId == playerId);
        }
        else if (campaignId is not null)
        {
            spinHistoryQuery = spinHistoryQuery.Where(x => x.CampaignId == campaignId);
        }
        else if (playerId is not null)
        {
            spinHistoryQuery = spinHistoryQuery.Where(x => x.PlayerId == playerId);
        }

        return await spinHistoryQuery
            .Select(x => new SpinHistoryDto
            {
                Id = x.Id,
                CampaignId = x.CampaignId,
                PlayerId = x.PlayerId,
                SpinDate = x.Date
            })
            .ToListAsync();
    }
}
