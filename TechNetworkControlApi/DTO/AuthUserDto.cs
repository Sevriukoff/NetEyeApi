using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.DTO;

public class AuthUserDto : UserDto
{
    public ICollection<RepairRequestDto>? RepairRequestsSubmitted { get; set; }
    public ICollection<RepairRequestDto>? RepairRequestsReceived { get; set; }
}