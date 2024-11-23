using FikusIn.Commands;
using FikusIn.Model.Documents;
using FikusIn.Utils;
using FikusIn.ViewModels;
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

        #endregion


        public MainViewModel() 
        {
            
            Documents = DocumentManager.GetDocuments();
        }



    }
}
