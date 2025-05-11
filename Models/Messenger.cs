using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FikusIn.Models
{
    public class Msg : Messenger { }
    public class Messenger
    {
        private static readonly Messenger _instance = new();
        public static Messenger Instance => _instance;

        protected Messenger() { }

        private void SendMessage(Message message)
        {
            MessageReceived?.Invoke(this, message);
        }

        public event EventHandler<Message>? MessageReceived;

        public static void ShowInfo(string messageText)
        {
            _instance.SendMessage(new Message(messageText, Message.MessageType.Info));
        }
        public static void ShowWarning(string messageText)
        {
            _instance.SendMessage(new Message(messageText, Message.MessageType.Warning));
        }
        public static void ShowError(string messageText)
        {
            _instance.SendMessage(new Message(messageText, Message.MessageType.Error));
        }

    }
}
