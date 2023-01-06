namespace TechNetworkControlApi.DTO;

public class RepairRequestDto
{
    public int TechEquipmentId { get; set; }
    public int UserFromId { get; set; }
    public int UserToId { get; set; }
    public string? Description { get; set; }
}