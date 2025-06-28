using Microsoft.AspNetCore.SignalR;

namespace ChatSinglar.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string chatId, string senderName, string encryptedText, string timestamp)
    {
        await Clients.Group(chatId).SendAsync("ReceiveMessage", senderName, encryptedText, timestamp);
    }

    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }
}