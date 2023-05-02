namespace TechNetworkControlApi.Common;

public static class AuthConstants
{
    public const string Issuer = "http://5.128.221.139:7119";
    public const string Audience = Issuer;
    public const string SecretKey = "this_is_super_secret_key_for_auth_clients";
}