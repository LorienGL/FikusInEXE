using FikusIn.Commands;
using FikusIn.Model.Documents;
using FikusIn.Utils;
using FikusIn.ViewModels;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public ObservableCollection<Document> Documents { get; private set; }
        public BindingList<Progress> ProgressList { get; private set; }

        private MessagesViewModel _messagesViewModel = new MessagesViewModel();
        public MessagesViewModel MessagesViewModel
        {
            get => _messagesViewModel;
            set => SetProperty(ref _messagesViewModel, value);
        }
        #endregion

        #region Commands
        public static ICommand NewDocument => new RelayCommand(
            (object? obj) => { DocumentManager.NewDocument(); },
            (object? obj) => { return true; }
        );

        public static ICommand CloseDocument => new RelayCommand(
            (object? obj) => { DocumentManager.CloseDocument(obj as Document); },
            (object? obj) => { return true; }
        );

        public static ICommand SetActiveDocument => new RelayCommand(
            (object? obj) => { DocumentManager.SetActiveDocument(obj as Document); },
            (object? obj) => { return true; }
        );


        #endregion

        #region Event Handlers

        #endregion


        public MainViewModel() 
        {            
            Documents = DocumentManager.GetDocuments();
            ProgressList = Progress.StartProgressList();

            var w1 = new BackgroundWorker();
            w1.DoWork += (sender, e) =>
            {
                using var progress = new Progress(0, 1000, false);
                for (int i = 0; i <= 1000; i++)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(10));
                    progress.Current = i;
                }
            };
            w1.RunWorkerAsync();

            var w2 = new BackgroundWorker();
            w2.DoWork += (sender, e) =>
            {
                using var progress = new Progress(0, 500, false);
                for (int i = 0; i <= 500; i++)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    progress.Current = i;
                }
            };
            w2.RunWorkerAsync();

            var w3 = new BackgroundWorker();
            w3.DoWork += (sender, e) =>
            {
                using var progress = new Progress(0, 250, true);
                for (int i = 0; i <= 250; i++)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    progress.Current = i;
                }
            };
            w3.RunWorkerAsync();


        }



    }
}
