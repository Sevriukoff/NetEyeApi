// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using TechNetworkControlApi.Infrastructure.Enums;

#pragma warning disable CS8618
namespace TechNetworkControlApi.Infrastructure.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Patronymic { get; set; }
    public string Phone { get; set; }
    public UserRole Role { get; set; }
    public DateTime RegistrationDate { get; set; }
    public Guid? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpirationDate { get; set; }
    public ICollection<RepairRequest>? RepairRequestsSubmitted { get; set; }
    public ICollection<RepairRequest>? RepairRequestsReceived { get; set; }
}