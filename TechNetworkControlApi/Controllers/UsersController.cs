using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;

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
    public IActionResult Get([FromQuery]string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Ok(ServerDbContext.Users.ToArray());
        }
        
        var user = ServerDbContext.Users.FirstOrDefault(u => u.Email == email);

        return user != null ? Ok(user) : NotFound(user);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        await ServerDbContext.Users.AddAsync(user);
        await ServerDbContext.SaveChangesAsync();
        return Ok();
    }
}