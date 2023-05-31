namespace TechNetworkControlApi.Common;

public static class AuthConstants
{
    public const string Issuer = "http://5.128.221.139:7119";
    public const string Audience = Issuer;
    public const string SecretKey = "JwtSecretKey";
    public const string AccessToken = "access_token";
    public const string RefreshToken = "refresh_token";
    
    public static DateTime AccessTokenExpiresDateTime => DateTime.Now.AddHours(1);
    public static DateTime RefreshTokenExpiresDateTime => DateTime.Now.AddDays(31);
    
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Tech = "Tech";
        public const string User = "User";
    }
}