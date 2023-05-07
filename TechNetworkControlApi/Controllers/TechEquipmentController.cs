using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nelibur.ObjectMapper;
using TechNetworkControlApi.Common;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Controllers;

[ApiController]
[Authorize(Policy = AuthConstants.UserRoles.Tech)]
[Route("/api/[controller]")]
public class TechEquipmentController : ControllerBase
{
    public ServerDbContext ServerDbContext { get; set; }
    
    public TechEquipmentController(ServerDbContext serverDbContext)
    {
        ServerDbContext = serverDbContext;
    }

    [Authorize(Policy = AuthConstants.UserRoles.User)]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var techEquip = await ServerDbContext.TechEquipments.FindAsync(id);
        
        if (techEquip == null)
            return NotFound($"Tech equipment with id {id} is not found");

        return Ok(TinyMapper.Map<TechEquipmentDto>(techEquip));
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(ServerDbContext.TechEquipments
            .Select(x => TinyMapper.Map<TechEquipmentDto>(x))
            .ToArray());
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TechEquipmentDto techEquipmentDto)
    {
        var isValidIp = IPAddress.TryParse(techEquipmentDto.IpAddress, out _);
        var isValidId = await ServerDbContext.TechEquipments.FindAsync(techEquipmentDto.Id) == null;

        if (!isValidId)
            return BadRequest($"Tech equipment with id {techEquipmentDto.Id} already exist");

        if (!isValidIp)
            return BadRequest($"Ip address \"{techEquipmentDto.IpAddress}\" is not valid");
            
        if (techEquipmentDto.SoftsId != null)
        {
            var softs = techEquipmentDto.SoftsId.Select(x => new TechEquipmentTechSoft
            {
                InstalledDate = DateTime.Now,
                TechEquipmentId = techEquipmentDto.Id,
                TechSoftId = x
            }).ToList();
        }
        
        var techEquip = TinyMapper.Map<TechEquipment>(techEquipmentDto);

        await ServerDbContext.TechEquipments.AddAsync(techEquip);
        await ServerDbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new {id = techEquipmentDto.Id}, techEquip.Id);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] TechEquipmentDto techEquipmentDto)
    {
        var techEquip = await ServerDbContext.TechEquipments.FindAsync(techEquipmentDto.Id);

        techEquip.IpAddress = techEquipmentDto.IpAddress;

        await ServerDbContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var techEquip = await ServerDbContext.TechEquipments.FindAsync(id);

        ServerDbContext.TechEquipments.Remove(techEquip);

        await ServerDbContext.SaveChangesAsync();
        
        return Ok();
    }
    
    // HTTP PUT
}