namespace TechNetworkControlApi.Infrastructure.Entities;

public class TechEquipmentTechSoft
{
    public DateTime InstalledDate { get; set; }

    public string TechEquipmentId { get; set; }
    public TechEquipment TechEquipment { get; set; }

    public int TechSoftId { get; set; }
    public TechSoft TechSoft { get; set; }
}