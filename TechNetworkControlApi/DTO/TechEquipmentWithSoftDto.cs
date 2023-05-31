using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.DTO;

public class TechEquipmentWithSoftDto
{
    public string Id { get; set; }
    public string IpAddress { get; set; }
    public TechType Type { get; set; }
    public int[]? SoftsId { get; set; }

    public long? TotalRepairRequest { get; set; }
}