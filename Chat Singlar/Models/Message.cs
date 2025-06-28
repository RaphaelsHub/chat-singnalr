namespace ChatSinglar.Models;

public class Message
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string EncryptedText { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}