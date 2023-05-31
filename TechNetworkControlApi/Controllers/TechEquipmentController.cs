using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using TechNetworkControlApi.Common;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;
using TechNetworkControlApi.Services;

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

    [Authorize(Policy = AuthConstants.UserRoles.User)]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var techEquip = await ServerDbContext.TechEquipments.FindAsync(id);
        
        if (techEquip == null)
            return NotFound($"Tech equipment with id {id} is not found");

        return Ok(TinyMapper.Map<TechEquipmentDto>(techEquip));
    }

    [Authorize(Policy = AuthConstants.UserRoles.User)]
    [HttpGet]
    public IActionResult GetAll()
    {
        var sqlQuery = @"
            SELECT 
                t.Id,
                t.IpAddress,
                t.Type,
                COUNT(r.Id) AS TotalRepairRequest
            FROM 
                TechEquipments t
            LEFT JOIN 
                RepairRequests r ON t.Id = r.TechEquipmentId
            GROUP BY 
                t.Id, t.IpAddress
        ";

        var rawResult = ServerDbContext.FromSqlQuery(sqlQuery, x =>
            {
                var tech = new TechEquipmentDto
                {
                    Id = x["Id"] is DBNull ? "" : (string) x["Id"],
                    IpAddress = x["IpAddress"] is DBNull ? "" : (string) x["IpAddress"],
                    TotalRepairRequest = x["TotalRepairRequest"] is DBNull ? 0 : (long) x["TotalRepairRequest"]
                };

                string type = x["Type"] is DBNull ? "" : (string) x["Type"];
                
                switch (type)
                {
                    case "Computer":
                        tech.Type = TechType.Computer;
                        break;
                    case "Printer":
                        tech.Type = TechType.Printer;
                        break;
                    case "Camera":
                        tech.Type = TechType.Camera;
                        break;
                }
                
                return tech;
            }
        );

        return Ok(rawResult);
    }
    
    [Authorize(Policy = AuthConstants.UserRoles.Tech)]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TechEquipmentWithSoftDto techEquipmentDto)
    {
        Thread.Sleep(3000);
        
        var isValidIp = Regex.IsMatch(techEquipmentDto.IpAddress, "^(?:[0-9]{1,3}\\.){3}[0-9]{1,3}$");
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

    [Authorize(Policy = AuthConstants.UserRoles.Tech)]
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] TechEquipmentDto techEquipmentDto)
    {
        var techEquip = await ServerDbContext.TechEquipments.FindAsync(techEquipmentDto.Id);

        if (techEquip == null)
            return BadRequest($"Tech equipment with id {techEquipmentDto.Id} is not found");
        
        var isValidIp = Regex.IsMatch(techEquipmentDto.IpAddress, "^(?:[0-9]{1,3}\\.){3}[0-9]{1,3}$");
        
        if (!isValidIp)
            return BadRequest($"Ip address \"{techEquipmentDto.IpAddress}\" is not valid");

        techEquip.IpAddress = techEquipmentDto.IpAddress;

        await ServerDbContext.SaveChangesAsync();
        
        return NoContent();
    }

    [Authorize(Policy = AuthConstants.UserRoles.Tech)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var techEquip = await ServerDbContext.TechEquipments.FindAsync(id);

        if (techEquip == null)
            return NotFound($"Tech equipment with id {id} is not found");
        
        ServerDbContext.TechEquipments.Remove(techEquip);

        await ServerDbContext.SaveChangesAsync();
        
        return NoContent();
    }
}