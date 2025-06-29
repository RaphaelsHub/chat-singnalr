using ChatSinglar.Abstractions;
using ChatSinglar.Models;
using Microsoft.AspNetCore.Identity;

namespace ChatSinglar.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthService(UserManager<IdentityUser> userManager,
                       SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }



    public async Task<IdentityUser?> LoginAsync(LoginDto model)
    {
        if (model == null) return null;

        var ExistingUser = await _userManager.FindByNameAsync(model.UserName);
        if (ExistingUser == null) return null;

        var result = await _signInManager.CheckPasswordSignInAsync(ExistingUser, model.Password, false);
        if (!result.Succeeded) return null;

        return ExistingUser;
    }
    public async Task<IdentityResult> RegisterAsync(RegisterDto model)
    {
        if (model == null)
            return IdentityResult.Failed(new IdentityError { Description = "Model cannot be null." });

        var existingUser = await _userManager.FindByNameAsync(model.UserName);
        if (existingUser != null)
            return IdentityResult.Failed(new IdentityError { Description = "User already exists." });

        var newUser = new IdentityUser { UserName = model.UserName };
        var result = await _userManager.CreateAsync(newUser, model.Password);

        return result;
    }
}
