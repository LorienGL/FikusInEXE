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

        private static readonly double[] WindowZoomFactors = { 1, 1.25, 1.50, 2 };

        #region Observed Properties
        public ObservableCollection<Document> Documents { get; private set; }
        public ObservableCollection<string> SortedDocumentNames { get; private set; }
        public BindingList<Progress> ProgressList { get; private set; }

        private MessagesViewModel _messagesViewModel = new MessagesViewModel();
        public MessagesViewModel MessagesViewModel
        {
            get => _messagesViewModel;
            set => SetProperty(ref _messagesViewModel, value);
        }

        private int _windowScaleIndex = 1;
        private double _windowScale = 1.25;
        public double WindowScale
        {
            get => WindowZoomFactors[_windowScaleIndex];
            set => SetProperty(ref _windowScale, value);
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

        public ICommand SetNextZoomFactor => new RelayCommand(
            (object? obj) => { _windowScaleIndex = (_windowScaleIndex == WindowZoomFactors.Length - 1? 0 : _windowScaleIndex + 1);  WindowScale = WindowZoomFactors[_windowScaleIndex]; },
            (object? obj) => { return true; }
        );

        #endregion

        #region Event Handlers

        #endregion



        public MainViewModel() 
        {            
            Documents = DocumentManager.GetDocuments();
            Documents.CollectionChanged += Documents_CollectionChanged;

            var docNames = Documents.Select(d => d.Name).ToList();
            docNames.Sort(StringComparer.CurrentCultureIgnoreCase);
            SortedDocumentNames = new ObservableCollection<string>(docNames);

            ProgressList = Progress.StartProgressList();
        }

        private void Documents_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var docNames = Documents.Select(d => d.Name).ToList();
            docNames.Sort(StringComparer.CurrentCultureIgnoreCase);
            SortedDocumentNames.Clear();
            foreach (var doc in docNames)
                SortedDocumentNames.Add(doc);
        }
    }
}
