﻿namespace TechNetworkControlApi.DTO;

public class UserChangePasswordDto
{
    public int Id { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}