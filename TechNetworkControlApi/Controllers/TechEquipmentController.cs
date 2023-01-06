using Microsoft.AspNetCore.Mvc;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class TechEquipmentController : ControllerBase
{
    public ServerDbContext ServerDbContext { get; set; }
    
    public TechEquipmentController(ServerDbContext serverDbContext)
    {
        ServerDbContext = serverDbContext;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] string id)
    {
        var techEquip = ServerDbContext.TechEquipments.FirstOrDefault(x => x.Id == id);
        
        if (techEquip == null)
        {
            return NotFound($"Tech equipment with id {id} is not found");
        }
        
        return Ok(techEquip);
    }

    [HttpPost]
    public IActionResult Post([FromBody] TechEquipmentDto techEquipmentDto)
    {
        var type = Enum.Parse<TechType>(techEquipmentDto.Type);
        var softs = techEquipmentDto.SoftsId.Select(x => new TechEquipmentTechSoft
        {
            InstalledDate = DateTime.Now,
            TechEquipmentId = techEquipmentDto.Id,
            TechSoftId = x
        }).ToList();
        
        var techEquip = new TechEquipment
        {
            Id = techEquipmentDto.Id,
            IpAddress = techEquipmentDto.IpAddress,
            Type = type,
            Softs = softs
        };

        ServerDbContext.TechEquipments.Add(techEquip);
        ServerDbContext.SaveChanges();
        
        return Ok();
    }
}