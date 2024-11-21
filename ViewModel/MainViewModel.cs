using FikusIn.Commands;
using FikusIn.Model.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FikusIn.ViewModel
{
    public class MainViewModel
    {
        #region Observed Objects
        public ObservableCollection<Document> Documents { get; set; }
        #endregion

        #region Observed Commands
        public ICommand NewDocument => new RelayCommand(
            (object? obj) => { DocumentManager.NewDocument(); },
            (object? obj) => { return true; }
        );

        public ICommand CloseDocument => new RelayCommand(
            (object? obj) => 
            { 
                DocumentManager.CloseDocument(obj as Document); 
            },
            (object? obj) => { return true; }
        );

        public ICommand SetActiveDocument => new RelayCommand(
            (object? obj) => { DocumentManager.SetActiveDocument(obj as Document); },
            (object? obj) => { return true; }
        );


        #endregion


        public MainViewModel() 
        {
            Documents = DocumentManager.GetDocuments();
        }



    }
}
