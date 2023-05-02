using Microsoft.EntityFrameworkCore;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.EntitiesTypeConfiguration;

namespace TechNetworkControlApi.Infrastructure;

public class ServerDbContext : DbContext
{
    public const string DbName = "TechNetworkControl";

    public DbSet<User> Users { get; set; }
    public DbSet<RepairRequest> RepairRequests { get; set; }
    public DbSet<TechEquipment> TechEquipments { get; set; }
    public DbSet<TechSoft> TechSofts { get; set; }

    public ServerDbContext()
    {
        
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql(
                $"server=37.140.192.90;user id=u2011310_remote;password=dC2pH6eM9vfV2pK4;database=u2011310_technetwork_control",
                ServerVersion.Parse("5.7.27-mysql"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RepairRequestTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TechEquipmentTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TechSoftTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TechEquipmentTechSoftTypeConfiguration());
    }
}