using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FikusIn.Utils
{
    public class Message(string text, Message.MessageType type)
    {
        public string Text { get; } = text;
        public MessageType Type { get; } = type;

        public enum MessageType
        {
            Error,
            Warning,
            Info
        }
    }
}
