using System;

namespace FytSoa.Common.Jwt.Model;

public class JwtToken
{
    public long Id { get; set; } = 0;

    public long TenantId { get; set; } = 0;
    
    public string? RoleArray { get; set; }

    public string? FullName { get; set; }

    public string? Role { get; set; }

    public DateTime Time { get; set; } = new();
}