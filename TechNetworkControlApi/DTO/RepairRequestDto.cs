using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.DTO;

public class RepairRequestDto
{
    public int? Id { get; set; }
    public string? TechEquipmentId { get; set; }
    public string? TechIpAddress { get; set; }
    public TechType? TechType { get; set; }
    public int? UserFromId { get; set; }
    public int? UserToId { get; set; }
    public DateTime CreatedDate { get; set; }
    public RepairRequestStatus? Status { get; set; }
    public string? Description { get; set; }
    public string? RepairNote { get; set; }
}

public class SingleRepairRequestDto
{
    public int? Id { get; set; }
    public string? TechEquipmentId { get; set; }
    public string? TechIpAddress { get; set; }
    public TechType? TechType { get; set; }
    public string UserFrom { get; set; }
    public string UserTo { get; set; }
    public string CreatedDate { get; set; }
    public RepairRequestStatus? Status { get; set; }
    public string? Description { get; set; }
}