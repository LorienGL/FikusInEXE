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

        public static void Send(string messageText, Message.MessageType messageType)
        {
            _instance.SendMessage(new Message(messageText, messageType));
        }

    }
}
