using System.Dynamic;
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
using RazorLight;
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
    private readonly ServerDbContext _serverDbContext;
    private readonly IEmailService _emailService;
    private readonly IRazorLightEngine _razorLightEngine;
    private readonly IWebHostEnvironment _hostEnvironment;

    public UsersController(ServerDbContext serverDbContext, IEmailService emailService,
        IRazorLightEngine razorLightEngine, IWebHostEnvironment hostEnvironment)
    {
        _serverDbContext = serverDbContext;
        _emailService = emailService;
        _razorLightEngine = razorLightEngine;
        _hostEnvironment = hostEnvironment;
    }
    
    [Authorize(Policy = AuthConstants.UserRoles.Tech)]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var user = _serverDbContext.Users
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
        return Ok(_serverDbContext.Users.Select(x => TinyMapper.Map<UserDto>(x)).ToArray());
    }

    [Authorize(Policy = AuthConstants.UserRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User user)
    {
        bool isValidEmail = !_serverDbContext.Users.Any(x => x.Email == user.Email);

        if (!isValidEmail)
            return BadRequest("Email already exist");

        await _serverDbContext.Users.AddAsync(user);
        await _serverDbContext.SaveChangesAsync();

        var renderTemplate = await _razorLightEngine.CompileRenderAsync("RegisterUserTemplate.cshtml", user);
        await _emailService.SendEmailWithAttachmentsAsync(user.Email, "Регистрация в системе Net-Eye", renderTemplate,
            Path.Combine(_hostEnvironment.ContentRootPath, "HtmlTemplates/Resources/QrCodeMobileApp.png"));
        
        return CreatedAtAction(nameof(Get), new {id = user.Id}, user.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromBody] UserChangePasswordDto userDto)
    {
        var dbUser = await _serverDbContext.Users.FindAsync(userDto.Id);

        if (dbUser == null)
            return NotFound();
        
        if (dbUser.Password != userDto.OldPassword)
            return BadRequest();

        dbUser.Password = userDto.NewPassword;
        await _serverDbContext.SaveChangesAsync();

        var renderTemplate = await _razorLightEngine.CompileRenderAsync("ResetPasswordTemplate.cshtml", dbUser);
        await _emailService.SendEmailAsync(dbUser.Email, "Сброс пароля", renderTemplate);
        
        return Ok(TinyMapper.Map<AuthUserDto>(dbUser));
    }

    [Authorize(Policy = AuthConstants.UserRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _serverDbContext.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        _serverDbContext.Users.Remove(user);
        await _serverDbContext.SaveChangesAsync();

        var renderTemplate = await _razorLightEngine.CompileRenderAsync("DeleteUserTemplate.cshtml", user);
        await _emailService.SendEmailAsync(user.Email, "Ваша учётная запись удалена", renderTemplate);

        return Ok();
    }
}