using ChatSinglar.Abstractions;
using ChatSinglar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatSinglar.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IJwtService _jwtService;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var user = new IdentityUser { UserName = model.UserName };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null) return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded) return Unauthorized();

        var token = _jwtService.GenerateToken(user);

        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok(new { Message = "You are authorized!" });
    }
}