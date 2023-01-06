namespace TechNetworkControlApi.Infrastructure.Entities;

public class TechSoft
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public ICollection<TechEquipment> TechEquipments { get; set; }
}