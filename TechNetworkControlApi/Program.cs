using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nelibur.ObjectMapper;
using RazorLight;
using RazorLight.Compilation;
using RazorLight.Extensions;
using TechNetworkControlApi.Common;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Entities;
using TechNetworkControlApi.Services;

var builder = WebApplication.CreateBuilder(args);

#region Configuration

builder.Host.ConfigureAppConfiguration(x => x.AddJsonFile("Config/settings.json", false, true));

builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

#endregion

#region Controllers

builder.Services.AddControllers(opt =>
    {
        opt.ValueProviderFactories.Add(new CookieValueProviderFactory());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.IgnoreNullValues = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    }
);

#endregion

#region Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

#endregion

#region Authentication

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Auth:Issuer"],
        ValidAudience = builder.Configuration["Auth:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Auth:JwtSecretKey"]))
    };

    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue(AuthConstants.AccessToken, out string token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

#endregion

#region Authorization

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(AuthConstants.UserRoles.Admin, config =>
    {
        config.RequireClaim(ClaimTypes.Role, AuthConstants.UserRoles.Admin);
    });
    
    opt.AddPolicy(AuthConstants.UserRoles.Tech, config =>
    {
        config.RequireAssertion(x =>
            x.User.HasClaim(ClaimTypes.Role, AuthConstants.UserRoles.Tech) ||
            x.User.HasClaim(ClaimTypes.Role, AuthConstants.UserRoles.Admin));
    });
    
    opt.AddPolicy(AuthConstants.UserRoles.User, config =>
    {
        config.RequireAssertion(
            x => x.User.HasClaim(ClaimTypes.Role, AuthConstants.UserRoles.User) || 
                 x.User.HasClaim(ClaimTypes.Role, AuthConstants.UserRoles.Tech) ||
                 x.User.HasClaim(ClaimTypes.Role, AuthConstants.UserRoles.Admin));
    });
});

#endregion

#region DependencyInjections

builder.Services.AddDbContext<ServerDbContext>(opt =>
{
    opt.UseMySql
    (
        builder.Configuration.GetConnectionString("MySqlProd"),
        ServerVersion.Parse("5.7.27-mysql")
    );
});

var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "HtmlTemplates");

builder.Services.AddSingleton<IRazorLightEngine>(provider =>
{
    var engine = new RazorLightEngineBuilder()
        .UseFileSystemProject(templatePath)
        .Build();
            
    return engine;
});

builder.Services.AddTransient<IEmailService, EmailService>();

#endregion

#region Sertificate

const string certificatePath = @"C:\Users\Bellatrix\RiderProjects\WPFProjects\TechNetworkControlApi\TechNetworkControlApi\bin\Debug\net6.0\LocalWin.pfx";
const string certificatePassword = "123321";

var certificate = new X509Certificate2(certificatePath, certificatePassword);

builder.Services.Configure<HttpsConnectionAdapterOptions>(opt =>
{
    //opt.ServerCertificate = certificate;
});

#endregion

var app = builder.Build();

app.UseRouting();

app.UseCors(
    options => options
        .WithOrigins("http://192.168.0.107:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .WithExposedHeaders("Server", AuthConstants.AccessToken, AuthConstants.RefreshToken)
);

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

ConfigureObjectsMapping();

app.Run();

void ConfigureObjectsMapping()
{
    TinyMapper.Bind<RepairRequest, RepairRequestDto>(config =>
    {
        config.Bind(s => s.TechEquipment.Id, t => t.TechEquipmentId);
        config.Bind(s => s.TechEquipment.IpAddress, t => t.TechIpAddress);
        config.Bind(s => s.TechEquipment.Type, t => t.TechType);
    });

    TinyMapper.Bind<User, AuthUserDto>();
    TinyMapper.Bind<User, UserDto>();
    TinyMapper.Bind<TechEquipmentDto, TechEquipment>();
    TinyMapper.Bind<TechEquipmentWithSoftDto, TechEquipment>();
    TinyMapper.Bind<TechEquipment, TechEquipmentDto>();
}