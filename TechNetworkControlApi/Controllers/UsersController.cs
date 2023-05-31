using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nelibur.ObjectMapper;
using TechNetworkControlApi.Common;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Infrastructure.Enums;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TechNetworkControlApi.Controllers;

[ApiController]
[Authorize]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    public ServerDbContext ServerDbContext { get; set; }

    public UsersController(ServerDbContext serverDbContext)
    {
        ServerDbContext = serverDbContext;
    }
    
    [Authorize(Policy = AuthConstants.UserRoles.Tech)]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var user = ServerDbContext.Users
            .Include(x => x.RepairRequestsReceived)!
                .ThenInclude(x => x.TechEquipment)
            .Include(x => x.RepairRequestsSubmitted)!
                .ThenInclude(x => x.TechEquipment)
            .FirstOrDefault(x => x.Id == id);
            
        if (user == null)
            return NotFound();

        return Ok(TinyMapper.Map<UserDto>(user));
    }

    [Authorize(Policy = AuthConstants.UserRoles.Admin)]
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(ServerDbContext.Users.Select(x => TinyMapper.Map<UserDto>(x)).ToArray());
    }

    [Authorize(Policy = AuthConstants.UserRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        await ServerDbContext.Users.AddAsync(user);
        await ServerDbContext.SaveChangesAsync();
        
        return CreatedAtAction(nameof(Get), new {id = user.Id}, user.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromBody] UserChangePasswordDto userDto)
    {
        var dbUser = await ServerDbContext.Users.FindAsync(userDto.Id);

        if (dbUser == null)
            return NotFound();
        
        if (dbUser.Password != userDto.OldPassword)
            return BadRequest();

        dbUser.Password = userDto.NewPassword;
        await ServerDbContext.SaveChangesAsync();
        return Ok(TinyMapper.Map<AuthUserDto>(dbUser));

    }

    [Authorize(Policy = AuthConstants.UserRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await ServerDbContext.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        ServerDbContext.Users.Remove(user);

        await ServerDbContext.SaveChangesAsync();

        return Ok();
    }
}