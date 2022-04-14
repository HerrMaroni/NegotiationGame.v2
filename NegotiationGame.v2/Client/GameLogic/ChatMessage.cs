using System;

namespace NegotiationGame.v2.Client.GameLogic
{
    public class ChatMessage
    {
        public ChatMessage(string message, DateTime time, string username)
        {
            Message = message;
            Time = time;
            Username = username;
        }

        public string Message { get; set; }
        public DateTime Time { get; set; }
        public string Username { get; set; }
    }
}