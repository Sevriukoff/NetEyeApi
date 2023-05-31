using System.IdentityModel.Tokens.Jwt;

namespace TechNetworkControlApi.Common;

public static class CommonExtensions
{
    public static string GetString(this JwtSecurityToken token) => new JwtSecurityTokenHandler().WriteToken(token);
}