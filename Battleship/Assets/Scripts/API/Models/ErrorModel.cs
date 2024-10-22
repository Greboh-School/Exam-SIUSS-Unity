using JetBrains.Annotations;

namespace Network.API.Models
{
    public class ErrorModel
    {
        public string Message { get; set; }
        [CanBeNull]
        public string StackTrace { get; set; }
    }
}