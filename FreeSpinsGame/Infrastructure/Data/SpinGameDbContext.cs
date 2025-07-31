using FreeSpinsGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FreeSpinsGame.Infrastructure.Data;

public class SpinGameDbContext(DbContextOptions<SpinGameDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; set; }
    
    public DbSet<Campaign> Campaigns { get; set; }
    
    public DbSet<PlayerCampaign> PlayerCampaigns { get; set; }
    
    public DbSet<SpinHistory> SpinHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<PlayerCampaign>()
            .HasKey(x => new { x.PlayerId, x.CampaignId });
        
        modelBuilder
            .Entity<PlayerCampaign>()
            .HasOne(x => x.Player)
            .WithMany(x => x.PlayerCampaigns)
            .HasForeignKey(x => x.PlayerId);
        
        modelBuilder
            .Entity<PlayerCampaign>()
            .HasOne(x => x.Campaign)
            .WithMany(x => x.PlayerCampaigns)
            .HasForeignKey(x => x.CampaignId);

        modelBuilder
            .Entity<Campaign>()
            .Property(x => x.MaxSpinCount)
            .HasDefaultValue(5);
    }
}