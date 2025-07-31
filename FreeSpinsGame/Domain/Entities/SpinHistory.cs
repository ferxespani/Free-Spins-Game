namespace FreeSpinsGame.Domain.Entities;

public class SpinHistory : BaseEntity
{
    public int PlayerId { get; set; }

    public Player Player { get; set; } = null!;
    
    public int CampaignId { get; set; }
    
    public Campaign Campaign { get; set; } = null!;
    
    public DateTime Date { get; set; }
}