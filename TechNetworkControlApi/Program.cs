using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthConstants.SecretKey))
    };
});

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(UserRole.Admin.ToString(), config =>
    {
        config.RequireClaim(ClaimTypes.Role, "Admin");
    });
    
    opt.AddPolicy(UserRole.Tech.ToString(), config =>
    {
        config.RequireAssertion(x =>
            x.User.HasClaim(ClaimTypes.Role, "Tech") ||
            x.User.HasClaim(ClaimTypes.Role, "Admin"));
    });
});

builder.Services.AddTransient<ServerDbContext>();

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