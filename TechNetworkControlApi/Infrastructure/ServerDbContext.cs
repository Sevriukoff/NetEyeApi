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

    public ServerDbContext(DbContextOptions<ServerDbContext> options): base(options)
    {
        
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