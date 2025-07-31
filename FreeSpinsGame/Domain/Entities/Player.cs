namespace FreeSpinsGame.Domain.Entities;

public class Player : BaseEntity
{
    public List<PlayerCampaign> PlayerCampaigns { get; set; } = [];
}