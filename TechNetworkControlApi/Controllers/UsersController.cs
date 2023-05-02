using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TechNetworkControlApi.Common;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;
using TechNetworkControlApi.Services;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TechNetworkControlApi.Controllers;

[ApiController]
[Authorize]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    public ServerDbContext ServerDbContext { get; set; }
    public HashServiceSha256 Sha256 { get; set; } = new HashServiceSha256();
    
    public UsersController(ServerDbContext serverDbContext)
    {
        ServerDbContext = serverDbContext;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] int? id, [FromQuery] UserRole? role)
    {
        User? user;

        if (id != null)
        {
            user = ServerDbContext.Users
                .Include(x => x.RepairRequestsReceived)!
                    .ThenInclude(x => x.TechEquipment)
                .Include(x => x.RepairRequestsSubmitted)!
                    .ThenInclude(x => x.TechEquipment)
                .FirstOrDefault(x => x.Id == id);
            
            if (user == null)
            {
                return NotFound();
            }

            return Ok(MapUser(user));
        }
        
        if (role != null)
        {
            Thread.Sleep(5000);
            
           var users = ServerDbContext.Users.Where(x => x.Role == role)
                .ToList();

            return Ok(users);
        }

        return Ok(ServerDbContext.Users.ToArray());
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        await ServerDbContext.Users.AddAsync(user);
        await ServerDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] User user)
    {
        var dbUser = await ServerDbContext.Users.FindAsync(user.Id);

        if (dbUser != null)
        {
            dbUser.Password = user.Password;
            await ServerDbContext.SaveChangesAsync();
            return Ok();
        }
        
        return NotFound();
    }

    [Authorize(Policy = "Admin")]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var user = await ServerDbContext.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }
        
        ServerDbContext.Users.Remove(user);

        await ServerDbContext.SaveChangesAsync();

        return Ok();
    }

    private UserDto MapUser(User userDto)
    {
        UserDto user = new UserDto()
        {
            Id = userDto.Id,
            Email = userDto.Email,
            Password = userDto.Password,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Patronymic = userDto.Patronymic,
            Phone = userDto.Phone,
            Role = userDto.Role,
            RepairRequestsSubmitted = MapUserRequests(userDto.RepairRequestsSubmitted),
            RepairRequestsReceived = MapUserRequests(userDto.RepairRequestsReceived)
        };

        ICollection<RepairRequestDto>? MapUserRequests(ICollection<RepairRequest>? repairRequests)
        {
            return repairRequests?.Select(x => new RepairRequestDto
            {
                Id = x?.Id,
                TechEquipmentId = x?.TechEquipment?.Id,
                TechIpAddress = x?.TechEquipment?.IpAddress,
                UserFromId = x?.UserFromId,
                UserToId = x?.UserToId,
                Status = x?.Status,
                Description = x?.Description
            }).ToList();
        }

        return user;
    }
}