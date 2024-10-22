namespace Assets.Scripts.API.Models.DTOs
{
    public enum MessageType
    {
        Unknown = 0,
        Public = 1,
        Private = 2,
    }

    public class MessageDTO
    {
        public MessageType Type { get; set; }
        public string Content { get; set; } = default!;
        public string? Sender { get; set; }
        public string? Recipient { get; set; }
    }
}