using Microsoft.AspNetCore.Mvc;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;

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
        // id
        // Desc
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RepairRequestDto repairRequestDto)
    {
        var techEquip = await ServerDbContext.TechEquipments.FindAsync(repairRequestDto.TechEquipmentId);
        
        if (techEquip == null)
            return NotFound($"Tech equipment with id {repairRequestDto.TechEquipmentId} is not found");
        
        var userFrom = await ServerDbContext.Users.FindAsync(repairRequestDto.UserFromId);
        
        if (userFrom == null)
            return NotFound($"User sender with id {repairRequestDto.UserFromId} is not found");

        var repairRequest = new RepairRequest
        {
            TechEquipment = techEquip,
            UserFrom = userFrom,
            UserTo = null,
            Description = repairRequestDto.Description,
            Status = RepairRequestStatus.Accepted
        };

        ServerDbContext.RepairRequests.Add(repairRequest);
        await ServerDbContext.SaveChangesAsync();

        return Ok(repairRequest.Id);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] RepairRequestDto repairRequestDto)
    {
        var repairRequest = await ServerDbContext.RepairRequests.FindAsync(repairRequestDto.Id);

        if (repairRequest == null)
        {
            return NotFound();
        }

        var userTo = await ServerDbContext.Users.FindAsync(repairRequestDto.UserToId);

        if (userTo == null)
        {
            return NotFound();
        }

        if (userTo.Role == UserRole.User)
        {
            return BadRequest();
        }

        repairRequest.UserTo = userTo;
        repairRequest.Status = RepairRequestStatus.Working;

        await ServerDbContext.SaveChangesAsync();
        
        return Ok(repairRequest.Id);
    }
}