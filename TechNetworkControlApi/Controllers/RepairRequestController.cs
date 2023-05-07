using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using TechNetworkControlApi.Common;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Controllers;

[ApiController]
[Authorize]
[Route("/api/[controller]")]
public class RepairRequestController : ControllerBase
{
    public ServerDbContext ServerDbContext { get; set; }
    
    public RepairRequestController(ServerDbContext serverDbContext)
    {
        ServerDbContext = serverDbContext;
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var repairRequest = ServerDbContext.RepairRequests
            .Include(x => x.TechEquipment)
            .FirstOrDefault(x => x.Id == id);
        
        if (repairRequest == null)
            return NotFound($"Repair request with id {id} is not found");

        return Ok(TinyMapper.Map<RepairRequestDto>(repairRequest));
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        // TechEquipmentId
        // IpAddress
        // Desc
        // Status

        Thread.Sleep(1000);

        return Ok(ServerDbContext.RepairRequests.Include(x => x.TechEquipment)
            .Select(x => TinyMapper.Map<RepairRequestDto>(x)));
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
            Status = RepairRequestStatus.Pending
        };

        ServerDbContext.RepairRequests.Add(repairRequest);
        await ServerDbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new {id = repairRequest.Id} ,repairRequest.Id);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] RepairRequestDto repairRequestDto)
    {
        var repairRequest = await ServerDbContext.RepairRequests.FindAsync(repairRequestDto.Id);

        if (repairRequest == null)
            return NotFound($"Repair request with id {repairRequestDto.Id} is not found");

        var userTo = await ServerDbContext.Users.FindAsync(repairRequestDto.UserToId);

        if ( (repairRequest.Status == RepairRequestStatus.Pending
            && repairRequestDto.Status == RepairRequestStatus.Cancelled) ||
            repairRequest.Status == RepairRequestStatus.Cancelled)
        {
            repairRequest.Status = RepairRequestStatus.Cancelled;
        }
        else
        {
            if (userTo == null)
                return NotFound($"User receiver with id {repairRequestDto.UserToId} is not found ");

            if (userTo.Role == UserRole.User)
                return BadRequest("User receiver must be a tech or an admin");
        }

        repairRequest.UserTo = userTo;
        repairRequest.Status = (RepairRequestStatus) repairRequestDto.Status!;

        if (!string.IsNullOrEmpty(repairRequestDto.RepairNote) &&
            repairRequest.RepairNote != repairRequestDto.RepairNote)
        {
            repairRequest.RepairNote = repairRequestDto.RepairNote;
        }

        await ServerDbContext.SaveChangesAsync();
        
        return Ok(repairRequest.Id);
    }

    [Authorize(Policy = AuthConstants.UserRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var repairRequest = await ServerDbContext.RepairRequests.FindAsync(id);

        if (repairRequest == null)
            return NotFound();

        ServerDbContext.RepairRequests.Remove(repairRequest);

        await ServerDbContext.SaveChangesAsync();
        
        return Ok();
    }
}