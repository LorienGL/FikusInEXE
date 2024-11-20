using FikusIn.Model.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FikusIn.ViewModel
{
    public class MainViewModel
    {
        public ObservableCollection<Document> Documents { get; set; }
        public MainViewModel() 
        {
            Documents = DocumentManager.GetDocuments();
        }



    }
}
