using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Infrastructure.Entities;

public class RepairRequest
{
    public int Id { get; set; }
    public TechEquipment TechEquipment { get; set; }
    public User UserFrom { get; set; }
    public User? UserTo { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public RepairRequestStatus Status { get; set; }
}