using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Infrastructure.Entities;

public class TechEquipment
{
    public string Id { get; set; }
    public string IpAddress { get; set; }
    public TechType Type { get; set; }
    public DateTime RegistrationDate { get; set; }
    public ICollection<TechEquipmentTechSoft> Softs { get; set; }
}