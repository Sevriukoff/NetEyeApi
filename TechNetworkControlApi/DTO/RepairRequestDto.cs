namespace TechNetworkControlApi.DTO;

public class RepairRequestDto
{
    public int? Id { get; set; }
    public string? TechEquipmentId { get; set; }
    public int? UserFromId { get; set; }
    public int UserToId { get; set; }
    public string? Description { get; set; }
}