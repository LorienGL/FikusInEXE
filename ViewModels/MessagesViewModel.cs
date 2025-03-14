using FikusIn.Models;
using FikusIn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace FikusIn.ViewModels
{
    public class MessagesViewModel: ObservableObjectBase
    {
        private static readonly TimeSpan _messageMinTimerSpan = TimeSpan.FromSeconds(3);
        private DispatcherTimer? _messageTimer = null;

        #region Observed Properties
        private string _message = "";
        public string Message
        {
            get => _message;
            set
            {
                SetProperty(ref _message, value);

                _messageTimer?.Stop();
                if(_messageTimer != null)
                    _messageTimer.Interval = CalculateReadingTime(Message);
                _messageTimer?.Start();
                ShowMessage = true;
            }
        }

        private bool _showMessage = false;
        public bool ShowMessage
        {
            get => _showMessage;
            set => SetProperty(ref _showMessage, value);
        }

        private Brush _messageForeground = Brushes.White;
        public Brush MessageForeground
        {
            get => _messageForeground;
            set => SetProperty(ref _messageForeground, value);
        }

        private Brush _messageBackground = Brushes.Black;
        public Brush MessageBackground
        {
            get => _messageBackground;
            set => SetProperty(ref _messageBackground, value);
        }
        #endregion

        #region Event Handlers
        private void MessageReceivedEventHandler(Object? sender, Models.Message message)
        {
            Message = message.Text;
            if (message.Type == Models.Message.MessageType.Info)
            {
                MessageBackground = Brushes.Black;
                MessageForeground = Brushes.White;
            }
            else if (message.Type == Models.Message.MessageType.Warning)
            {
                MessageBackground = Brushes.Yellow;
                MessageForeground = Brushes.Black;
            }
            else if (message.Type == Models.Message.MessageType.Error)
            {
                MessageBackground = Brushes.DarkRed;
                MessageForeground = Brushes.White;
            }
        }

        private void ActiveDocumentChangedEventHandler(Object? sender, Document? doc)
        {

        }

        #endregion

        public static TimeSpan CalculateReadingTime(string text, int wordsPerMinute = 100) // Should be 200, but we asume users wont read very fast
        {
            var ts = TimeSpan.FromSeconds(text.Split(' ').Length * 60 / wordsPerMinute);
            return ts > _messageMinTimerSpan? ts: _messageMinTimerSpan;
        }

        public MessagesViewModel()
        {
            _messageTimer = new DispatcherTimer { Interval = _messageMinTimerSpan };
            _messageTimer.Tick += (sender, args) =>
            {
                ShowMessage = false;
                _messageTimer.Stop();
            };

            Messenger.Instance.MessageReceived += MessageReceivedEventHandler;
            Message = DateTime.Now.Hour < 12 ? "Good Morning!" : DateTime.Now.Hour < 18 ? "Good afternoon!" : "Good Evening!";
        }
    }
}
