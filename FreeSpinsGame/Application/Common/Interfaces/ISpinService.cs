using FreeSpinsGame.Web.Dtos;

namespace FreeSpinsGame.Application.Common.Interfaces;

public interface ISpinService
{
    Task<SpinResultDto> SpinAsync(int campaignId, int playerId);

    Task<SpinDataDto?> GetSpinDataAsync(int campaignId, int playerId);

    Task<List<SpinHistoryDto>> GetSpinHistoryAsync(int? campaignId, int? playerId);
}