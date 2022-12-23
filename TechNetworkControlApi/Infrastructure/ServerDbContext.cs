using Microsoft.EntityFrameworkCore;
using TechNetworkControlApi.Infrastructure.Entities;

namespace TechNetworkControlApi.Infrastructure;

public class ServerDbContext : DbContext
{
    public const string DbName = "TechNetworkControl";

    public DbSet<User> Users { get; set; }

    public ServerDbContext()
    {
        //Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql(
                $"server=localhost;user id=root;password=;database=u1478686_{DbName}",
                ServerVersion.Parse("5.7.39-mysql"));
        }
    }
}