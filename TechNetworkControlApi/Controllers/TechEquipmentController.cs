using Microsoft.AspNetCore.Mvc;
using TechNetworkControlApi.Infrastructure;

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
    public IActionResult Post()
    {
        return Ok();
    }
}