using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;

namespace TechNetworkControlApi.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    public ServerDbContext ServerDbContext { get; set; }
    
    public UsersController(ServerDbContext serverDbContext)
    {
        ServerDbContext = serverDbContext;
    }
    
    [HttpGet]
    public IActionResult Get([FromQuery]string? email,
        [FromQuery] int? id,
        [FromQuery] UserRole? role)
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
           var users = ServerDbContext.Users.Where(x => x.Role == role)
                .ToList();

            return Ok(users);
        }
        
        if (string.IsNullOrEmpty(email))
        {
            return Ok(ServerDbContext.Users.ToArray());
        }
        
        user = ServerDbContext.Users.Include(x => x.RepairRequestsSubmitted)
            .FirstOrDefault(u => u.Email == email);

        return user != null ? Ok(user) : NotFound(user);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        await ServerDbContext.Users.AddAsync(user);
        await ServerDbContext.SaveChangesAsync();
        return Ok();
    }

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
                Id = x.Id,
                TechEquipmentId = x.TechEquipment.Id,
                TechIpAddress = x.TechEquipment.IpAddress,
                UserFromId = x.UserFromId,
                UserToId = x.UserToId,
                Status = x.Status,
                Description = x.Description
            }).ToList();
        }

        return user;
    }
}