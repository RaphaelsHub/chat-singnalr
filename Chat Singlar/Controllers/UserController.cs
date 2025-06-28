using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatSinglar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserController(UserManager<IdentityUser> userManager) => _userManager = userManager;

    [Authorize]
    [HttpGet("search")]
    public IActionResult Search(string name)
    {
        var users = _userManager.Users
            .Where(u => u.UserName!.Contains(name))
            .Select(u => new { u.Id, u.UserName })
            .ToList();

        return Ok(users);
    }
}
