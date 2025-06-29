using ChatSinglar.Abstractions;
using ChatSinglar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSinglar.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController(IJwtService jwtService, IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IJwtService _jwtService = jwtService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var response = await _authService.RegisterAsync(model);
        return !response.Succeeded
            ? BadRequest(response.Errors)
            : Ok(new { Message = "User registered successfully." });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var response = await _authService.LoginAsync(model);
        if (response == null) return Unauthorized();

        var token = _jwtService.GenerateToken(response);
        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok(new { Message = "You are authorized!" });
    }
}