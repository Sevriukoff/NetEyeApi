﻿using TechNetworkControlClient.Enums;

namespace TechNetworkControlClient.DTO;

public class AuthUserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Patronymic { get; set; }
    public string Phone { get; set; }
    public UserRole Role { get; set; }
    public DateTime RegistrationDate { get; set; }
    public ICollection<RepairRequestDto> RepairRequestsSubmitted { get; set; }
    public ICollection<RepairRequestDto> RepairRequestsReceived { get; set; }
    
    public string FullName => string.Join(" ", FirstName, LastName, Patronymic);
}