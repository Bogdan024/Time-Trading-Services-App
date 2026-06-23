using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser
{
    public required string DisplayName { get; set; }
    public string? ImageUrl { get; set; }

    public Member Member { get; set; } = null!;
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
