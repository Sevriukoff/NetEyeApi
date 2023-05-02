using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ServerDbContext _serverDbContext;
    private readonly IConfiguration _config;
    
    public AuthController(ServerDbContext serverDbContext, IConfiguration config)
    {
        _serverDbContext = serverDbContext;
        _config = config;
    }
    
    [HttpGet]
    public async Task<IActionResult> Authorization([FromQuery] string email, [FromQuery] string password)
    {
        var user = await _serverDbContext.Users
            .Include(x => x.RepairRequestsSubmitted)!
                .ThenInclude(x => x.TechEquipment)
            .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

        if (user == null)
            return NotFound();

        var accessToken = CreateAccessToken(user);

        user.RefreshToken = Guid.NewGuid();
        user.RefreshTokenExpirationDate = DateTime.Now.AddDays(31);
        
        var userDto = TinyMapper.Map<AuthUserDto>(user);
        userDto.AccessToken = accessToken;
        userDto.RefreshToken = user.RefreshToken.ToString();

        await _serverDbContext.SaveChangesAsync();
        
        return Ok(userDto);
    }
    
    [HttpPut]
    public async Task<IActionResult> Refresh([FromBody] JwtDto jwtDto)
    {
        var user = await _serverDbContext.Users.FindAsync(jwtDto.UserId);

        if (user == null)
            return NotFound();

        bool isTokenValid = user.RefreshToken == Guid.Parse(jwtDto.RefreshToken) &&
                                user.RefreshTokenExpirationDate > DateTime.Now;

        if (!isTokenValid)
            return BadRequest();

        var refreshToken = Guid.NewGuid();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = DateTime.Now.AddDays(31);

        await _serverDbContext.SaveChangesAsync();
        
        return Ok(new JwtDto
        {
            UserId = user.Id,
            AccessToken = CreateAccessToken(user),
            RefreshToken = refreshToken.ToString() 
        });
    }

    private string CreateAccessToken(User user)
    {
        var userRoleStr = user.Role switch
        {
            UserRole.User => AuthConstants.UserRoles.User,
            UserRole.Tech => AuthConstants.UserRoles.Tech,
            UserRole.Admin => AuthConstants.UserRoles.Admin,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,
                string.Concat(user.LastName, user.FirstName, user.Patronymic)),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, userRoleStr)
        };

        var keyStr = _config.GetValue<string>(AuthConstants.SecretKey);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        
        var accessToken = new JwtSecurityToken
        (
            AuthConstants.Issuer,
            AuthConstants.Audience,
            claims, 
            DateTime.Now,
            DateTime.Now.AddMinutes(15),
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(accessToken);
    }
}