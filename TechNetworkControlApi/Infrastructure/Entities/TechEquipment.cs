﻿using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Infrastructure.Entities;

public class TechEquipment
{
    public string Id { get; set; }
    public string IpAddress { get; set; }
    public TechType Type { get; set; }
    public ICollection<TechSoft> Softs { get; set; }
}