namespace FreeSpinsGame.Domain.Entities;

public class Campaign : BaseEntity
{
    public int MaxSpinCount { get; set; }
    
    public List<PlayerCampaign> PlayerCampaigns { get; set; } = [];
}