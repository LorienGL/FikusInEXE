using FikusIn.Commands;
using FikusIn.Model.Documents;
using FikusIn.Utils;
using FikusIn.ViewModels;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace FikusIn.ViewModel
{
    public class MainViewModel: ObservableObjectBase
    {

        private static readonly double[] WindowZoomFactors = { 1, 1.2, 1.50, 2 };

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

        private double _windowScale = Properties.Settings.Default.WindowScale;
        public double WindowScale
        {
            get => _windowScale;
            set { SetProperty(ref _windowScale, value); Properties.Settings.Default.WindowScale = value; Properties.Settings.Default.Save(); }
        }

        private bool _wireframe = Properties.Settings.Default.Wireframe;
        public bool Wireframe
        {
            get => _wireframe;
            set { SetProperty(ref _wireframe, value); Properties.Settings.Default.Wireframe = value; Properties.Settings.Default.Save(); }
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

        public static ICommand SaveDocument => new RelayCommand(
            (object? obj) => { DocumentManager.SaveActiveDocument(); },
            (object? obj) => { return true; }
        );

        public static ICommand SaveDocumentAs => new RelayCommand(
            (object? obj) => { DocumentManager.SaveActiveDocument(); },
            (object? obj) => { return true; }
        );

        public static ICommand SaveAllDocuments => new RelayCommand(
            (object? obj) => { DocumentManager.SaveAllDocuments(); },
            (object? obj) => { return true; }
        );

        public static ICommand OpenDocument => new RelayCommand(
            (object? obj) => 
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = "Supported Files (*.fikus, *.dwg, *.dxf, *.stp, *.step, *.igs, *.iges, *.brep, *.obj)|*.fikus;*.dwg;*.dxf;*.stp;*.step;*.igs;*.iges;*.brep;*.obj";
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == true)
                    foreach (var path in dlg.FileNames)
                        DocumentManager.OpenDocument(path);
            },
            (object? obj) => { return true; }
        );


        public static ICommand SetActiveDocument => new RelayCommand(
            (object? obj) => { DocumentManager.SetActiveDocument(obj as Document); },
            (object? obj) => { return true; }
        );

        public ICommand SetSmallZoomFactor => new RelayCommand(
            (object? obj) => { WindowScale = WindowZoomFactors[0]; },
            (object? obj) => { return true; }
        );

        public ICommand SetMidZoomFactor => new RelayCommand(
            (object? obj) => { WindowScale = WindowZoomFactors[1]; },
            (object? obj) => { return true; }
        );

        public ICommand SetBigZoomFactor => new RelayCommand(
            (object? obj) => { WindowScale = WindowZoomFactors[2]; },
            (object? obj) => { return true; }
        );

        public ICommand SetHugeZoomFactor => new RelayCommand(
            (object? obj) => { WindowScale = WindowZoomFactors[3]; },
            (object? obj) => { return true; }
        );

        public ICommand ToggleWireframe => new RelayCommand(
            (object? obj) => { Wireframe = !Wireframe; },
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
