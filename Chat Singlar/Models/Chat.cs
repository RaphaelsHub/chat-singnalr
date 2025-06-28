namespace ChatSinglar.Models;

public class Chat
{
    public int Id { get; set; }
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public List<Message> Messages { get; set; } = new List<Message>();
}