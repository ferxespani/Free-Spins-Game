using FreeSpinsGame.Domain.Enums;

namespace FreeSpinsGame.Web.Dtos;

public class SpinResultDto
{
    public SpinStatus Status { get; set; }
    
    public int? CurrentCount { get; set; }

    public static SpinResultDto LimitReached { get; } = new() { Status = SpinStatus.LimitReached };
    public static SpinResultDto LockFailed { get; } = new() { Status = SpinStatus.Locked };
}