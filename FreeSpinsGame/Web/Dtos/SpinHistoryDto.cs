namespace FreeSpinsGame.Web.Dtos;

public class SpinHistoryDto
{
    public int Id { get; set; }
    
    public int PlayerId { get; set; }
    
    public int CampaignId { get; set; }
    
    public DateTime SpinDate { get; set; }
}