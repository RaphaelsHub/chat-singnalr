using Microsoft.AspNetCore.Identity;

namespace ChatSinglar.Abstractions;

public interface IJwtService
{
    string GenerateToken(IdentityUser user);
    bool ValidateToken(string token);
}