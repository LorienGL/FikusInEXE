using FikusIn.Commands;
using FikusIn.Model.Documents;
using FikusIn.Utils;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace FikusIn.ViewModel
{
    public class MainViewModel: ObservableObjectBase
    {
        #region Observed Properties
        public ObservableCollection<Document> Documents { get; set; }

        private string _message = "";
        public string Message
        {
            get => _message;
            set
            {
                if (!SetField(ref _message, value))
                    return;

                // start timer
                _isMessageFadingOut = true;
            }
        }

        private bool _isMessageFadingOut = false;
        public bool IsMessageFadingOut
        {
            get => _isMessageFadingOut;
            set
            {
                if (!SetField(ref _isMessageFadingOut, value))
                    return;

                // start timer
            }
        }

        private Brush _messageForeground = Brushes.White;
        public Brush MessageForeground
        {
            get => _messageForeground;
            set => SetField(ref _messageForeground, value);
        }

        private Brush _messageBackground = Brushes.Black;
        public Brush MessageBackground
        {
            get => _messageBackground;
            set => SetField(ref _messageBackground, value);
        }



        #endregion

        #region Commands
        public static ICommand NewDocument => new RelayCommand(
            (object? obj) => { DocumentManager.NewDocument(); },
            (object? obj) => { return true; }
        );

        public static ICommand CloseDocument => new RelayCommand(
            (object? obj) => 
            { 
                DocumentManager.CloseDocument(obj as Document); 
            },
            (object? obj) => { return true; }
        );

        public static ICommand SetActiveDocument => new RelayCommand(
            (object? obj) => { DocumentManager.SetActiveDocument(obj as Document); },
            (object? obj) => { return true; }
        );


        #endregion

        #region Event Handlers

        private void MessageReceivedEventHandler(Object? sender, Message message)
        {
            Message = message.Text;
            if(message.Type == Utils.Message.MessageType.Info) 
            {
                MessageBackground = Brushes.Black;
                MessageForeground = Brushes.White;
            } 
            else if(message.Type == Utils.Message.MessageType.Warning)
            {
                MessageBackground = Brushes.Yellow;
                MessageForeground = Brushes.Black;
            }
            else if(message.Type == Utils.Message.MessageType.Error)
            {
                MessageBackground = Brushes.DarkRed;
                MessageForeground = Brushes.White;
            }
        }

        #endregion


        public MainViewModel() 
        {
            Messenger.Instance.MessageReceived += MessageReceivedEventHandler;

            Documents = DocumentManager.GetDocuments();
            Message = DateTime.Now.Hour < 12 ? "Good Morning!" : DateTime.Now.Hour < 18 ? "Good afternoon!" : "Good Night!";
        }



    }
}
