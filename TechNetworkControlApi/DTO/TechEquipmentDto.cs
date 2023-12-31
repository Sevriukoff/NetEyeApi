﻿using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.DTO;

public class TechEquipmentDto
{
    public string Id { get; set; }
    public string IpAddress { get; set; }
    public TechType Type { get; set; }

    public long TotalRepairRequest { get; set; }
}