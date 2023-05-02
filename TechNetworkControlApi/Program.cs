using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TechNetworkControlApi.Common;
using TechNetworkControlApi.Infrastructure;
using TechNetworkControlApi.Infrastructure.Enums;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.IgnoreNullValues = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

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
        ValidIssuer = AuthConstants.Issuer,
        ValidAudience = AuthConstants.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>(AuthConstants.SecretKey)))
    };
});

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
});

builder.Services.AddDbContext<ServerDbContext>(opt =>
{
    opt.UseMySql
        (
            builder.Configuration.GetConnectionString("MySqlProd"),
            ServerVersion.Parse("5.7.27-mysql")
        );
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(
    options => options
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
);

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();