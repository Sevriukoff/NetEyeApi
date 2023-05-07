namespace TechNetworkControlApi.DTO;

public class JwtDto
{
    public string? AccessToken { get; set; }
    public string RefreshToken { get; set; }
}