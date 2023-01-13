using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public IActionResult Get([FromQuery] int? id)
    {
        // TechEquipmentId
        // IpAddress
        // Desc
        // Status

        if (id == null)
        {
            return Ok(ServerDbContext.RepairRequests.Include(x => x.TechEquipment)
                .Select(x => new RepairRequestDto
                {
                    Id = x.Id,
                    TechEquipmentId = x.TechEquipment.Id,
                    TechIpAddress = x.TechEquipment.IpAddress,
                    UserFromId = x.UserFromId,
                    UserToId = x.UserToId,
                    Description = x.Description
                }));
        }

        var repairRequest = ServerDbContext.RepairRequests
            .Include(x => x.TechEquipment)
            .FirstOrDefault(x => x.Id == id);

        if (repairRequest == null)
        {
            return NotFound();
        }
        
        return Ok(new RepairRequestDto
        {
            Id = repairRequest.Id,
            TechEquipmentId = repairRequest.TechEquipment.Id,
            TechIpAddress = repairRequest.TechEquipment.IpAddress,
            UserFromId = repairRequest.UserFromId,
            UserToId = repairRequest.UserToId,
            Description = repairRequest.Description
        });
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
            return NotFound($"Repair request with id {repairRequestDto.Id} is not found");
        }

        var userTo = await ServerDbContext.Users.FindAsync(repairRequestDto.UserToId);

        if (userTo == null)
        {
            return NotFound($"User receiver with id {repairRequestDto.UserToId} is not found ");
        }

        if (userTo.Role == UserRole.User)
        {
            return BadRequest("User receiver must be a tech or an admin");
        }

        repairRequest.UserTo = userTo;
        repairRequest.Status = RepairRequestStatus.Working;

        await ServerDbContext.SaveChangesAsync();
        
        return Ok(repairRequest.Id);
    }
}