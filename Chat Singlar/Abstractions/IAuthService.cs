using ChatSinglar.Models;
using Microsoft.AspNetCore.Identity;

namespace ChatSinglar.Abstractions;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterDto model);
    Task<IdentityUser?> LoginAsync(LoginDto model);
}