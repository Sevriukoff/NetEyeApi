using Microsoft.AspNetCore.Mvc;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;

namespace TechNetworkControlApi.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class RepairRequestController : ControllerBase
{
    public ServerDbContext ServerDbContext { get; set; }
    
    public RepairRequestController(ServerDbContext serverDbContext)
    {
        ServerDbContext = serverDbContext;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] int id)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult Post([FromBody] RepairRequestDto repairRequest)
    {
        return Ok();
    }

    [HttpPut]
    public IActionResult Put([FromBody] RepairRequestDto repairRequestDto)
    {
        return Ok();
    }
}