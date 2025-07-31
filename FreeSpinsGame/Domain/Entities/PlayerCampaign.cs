namespace FreeSpinsGame.Domain.Entities;

public class PlayerCampaign
{
    public int PlayerId { get; set; }

    public Player Player { get; set; } = null!;
    
    public int CampaignId { get; set; }
    
    public Campaign Campaign { get; set; } = null!;
    
    public int CurrentSpinCount { get; set; }
}