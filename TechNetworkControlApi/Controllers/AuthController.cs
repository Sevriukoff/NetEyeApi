using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    #region Fields
    
    private readonly ServerDbContext _serverDbContext;
    private readonly IOptions<AuthSettings> _authConfig;
    private readonly IEmailService _emailService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IRazorLightEngine _razorLightEngine;
    
    #endregion
    
    public AuthController(ServerDbContext serverDbContext, IOptions<AuthSettings> authConfig, IEmailService emailService,
        IHostEnvironment hostEnvironment, IRazorLightEngine razorLightEngine)
    {
        _serverDbContext = serverDbContext;
        _authConfig = authConfig;
        _emailService = emailService;
        _hostEnvironment = hostEnvironment;
        _razorLightEngine = razorLightEngine;
    }

    #region HttpMethods

    #region GetByCredentials
    
    [HttpGet("by-credentials")]
    public async Task<IActionResult> Authorization([FromQuery] string email, [FromQuery] string password)
    {
        var user = await _serverDbContext.Users
            .Include(x => x.RepairRequestsSubmitted)!
                .ThenInclude(x => x.TechEquipment)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return NotFound($"User with email {email} is not found");

        if (user.Password != password)
            return NotFound("User password is not correct");

        var userAccessToken = CreateAccessToken(user);

        user.RefreshToken = Guid.NewGuid();
        user.RefreshTokenExpirationDate = AuthConstants.RefreshTokenExpiresDateTime;
        
        var userDto = TinyMapper.Map<AuthUserDto>(user);

        await _serverDbContext.SaveChangesAsync();
        
        Response.Headers.Append(AuthConstants.AccessToken, userAccessToken.GetString());
        Response.Headers.Append(AuthConstants.RefreshToken, user.RefreshToken.ToString());
        
        SetAccessToken(userAccessToken);
        SetRefreshToken(user.RefreshToken.Value, user.RefreshTokenExpirationDate.Value);
        
        return Ok(userDto);
    }
    
    #endregion

    #region GetByCookie
    
    [HttpGet("by-cookie")]
    public IActionResult Authorization([FromCookie] string refresh_token)
    {
        var user = _serverDbContext.Users
            .Include(x => x.RepairRequestsSubmitted)!
                .ThenInclude(x => x.TechEquipment)
            .FirstOrDefault(x => x.RefreshToken.ToString() == refresh_token);

        if (user == null)
            return NotFound();

        if (user.RefreshTokenExpirationDate < DateTime.Now)
            return BadRequest("Need authorization");

        var userDto = TinyMapper.Map<AuthUserDto>(user);
        
        return Ok(userDto);
    }
    
    #endregion

    #region Post
    
    // TODO: Refactoring, before 21.06.23
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] UserDto registerRequest)
    {
        bool isValidEmail = !_serverDbContext.Users.Any(x => x.Email == registerRequest.Email);

        if (!isValidEmail)
            return BadRequest($"User with email {registerRequest.Email} already exist");

        var renderTemplate =
            await _razorLightEngine.CompileRenderAsync("RegisterRequestUserTemplate.cshtml", registerRequest);
        await _emailService.SendEmailAsync(_emailService.CorporationEmail, "Запрос на регистрацию", renderTemplate);

        return Ok();
    }
    
    #endregion

    #region Put

    [HttpPut]
    public async Task<IActionResult> Refresh([FromCookie] string refresh_token)
    {
        var user = await _serverDbContext.Users
            .FirstOrDefaultAsync(x => x.RefreshToken.ToString() == refresh_token);

        if (user == null)
            return NotFound();

        bool isTokenValid = user.RefreshToken == Guid.Parse(refresh_token) &&
                                user.RefreshTokenExpirationDate > DateTime.Now;

        if (!isTokenValid)
            return BadRequest();

        var userAccessToken = CreateAccessToken(user);
        var userRefreshToken = Guid.NewGuid();

        user.RefreshToken = userRefreshToken;
        user.RefreshTokenExpirationDate = AuthConstants.RefreshTokenExpiresDateTime;

        await _serverDbContext.SaveChangesAsync();
        
        SetAccessToken(userAccessToken);
        SetRefreshToken(user.RefreshToken.Value, user.RefreshTokenExpirationDate.Value);
        
        return NoContent();
    }
    
    #endregion

    #region Head
    
    [HttpHead]
    public IActionResult CheckAuthorization()
    {
        bool isExistAccessToken = Request.Cookies.ContainsKey(AuthConstants.AccessToken);
        bool isExistRefreshToken = Request.Cookies.ContainsKey(AuthConstants.RefreshToken);

        if (!isExistRefreshToken)
            return Unauthorized();

        return Ok();
    }
    
    #endregion
    
    #endregion

    #region PrivateMethods
    
    private void SetAccessToken(JwtSecurityToken accessToken)
    {
        Response.Cookies.Append(AuthConstants.AccessToken, accessToken.GetString(), new CookieOptions
        {
            HttpOnly = true,
            Expires = accessToken.ValidTo,
            Secure = false,
            SameSite = SameSiteMode.Strict,
        });
    }

    private void SetRefreshToken(Guid refreshToken, DateTime expires)
    {
        Response.Cookies.Append(AuthConstants.RefreshToken, refreshToken.ToString(), new CookieOptions
        {
            IsEssential = true,
            HttpOnly = true,
            Expires = expires,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth"
        });
    }
    
    private JwtSecurityToken CreateAccessToken(User user)
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
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name,
                string.Join(" ", user.LastName, user.FirstName, user.Patronymic)),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, userRoleStr)
        };

        var keyStr = _authConfig.Value.JwtSecretKey;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        
        var accessToken = new JwtSecurityToken
        (
            _authConfig.Value.Issuer,
            _authConfig.Value.Audience,
            claims, 
            DateTime.Now,
            AuthConstants.AccessTokenExpiresDateTime,
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return accessToken;
    }
    
    #endregion
}