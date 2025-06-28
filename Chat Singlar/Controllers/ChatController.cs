using System.Security.Claims;
using ChatSinglar.Data;
using ChatSinglar.Hubs;
using ChatSinglar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatSinglar.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub> _hub;

    public ChatController(AppDbContext db, UserManager<IdentityUser> userManager, IHubContext<ChatHub> hub)
    {
        _db = db;
        _userManager = userManager;
        _hub = hub;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartChat(string targetUserIdString)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserId = int.TryParse(currentUserIdString, out var parsedCurrentUserId) ? parsedCurrentUserId : 0;
        var targetUserId = int.TryParse(targetUserIdString, out var parsedTargetUserId) ? parsedTargetUserId : 0;

        if (currentUserId == 0) return Unauthorized();

        if (targetUserId == 0) return BadRequest("Invalid target user ID.");

        var existingChat = _db.Chats.FirstOrDefault(c =>
            (c.User1Id == currentUserId && c.User2Id == targetUserId) ||
            (c.User2Id == currentUserId && c.User1Id == targetUserId));

        if (existingChat != null) return Ok(existingChat.Id);

        var chat = new Chat { User1Id = currentUserId, User2Id = targetUserId, Messages = new List<Message>() };
        _db.Chats.Add(chat);
        await _db.SaveChangesAsync();
        return Ok(chat.Id);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(int chatId, string encryptedText)
    {
        var senderIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var senderId = int.TryParse(senderIdString, out var parsedSenderId) ? parsedSenderId : 0;

        if (senderId == 0 || string.IsNullOrEmpty(senderIdString)) return Unauthorized();

        var message = new Message { ChatId = chatId, SenderId = senderId, EncryptedText = encryptedText, Timestamp = DateTime.UtcNow };

        var sender = await _userManager.FindByIdAsync(senderIdString);

        if (sender == null) return Unauthorized();

        _db.Messages.Add(message);

        await _db.SaveChangesAsync();

        await _hub.Clients.Group(chatId.ToString())
       .SendAsync("ReceiveMessage", sender.UserName, encryptedText, message.Timestamp.ToString("o"));

        return Ok();
    }

    [HttpGet("{chatId}")]
    public IActionResult GetMessages(int chatId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.TryParse(userIdString, out var parsedUserId) ? parsedUserId : 0;

        if (userId == 0) return Unauthorized();

        var chat = _db.Chats.Include(c => c.Messages).FirstOrDefault(c => c.Id == chatId);

        if (chat == null || (chat.User1Id != userId && chat.User2Id != userId))
            return Unauthorized();

        return Ok(chat.Messages);
    }
}
