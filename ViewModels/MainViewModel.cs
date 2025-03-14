using FikusIn.Commands;
using FikusIn.Model.Documents;
using FikusIn.Models;
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

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
            set 
            { 
                SetProperty(ref _windowScale, value);
                foreach (var doc in Documents)
                    if(doc != null)
                        doc.WindowScaleInverted = 1.0 / value;
                Properties.Settings.Default.WindowScale = value; 
                Properties.Settings.Default.Save(); 
            }
        }

        private bool _wireframe = Properties.Settings.Default.Wireframe;
        public bool Wireframe
        {
            get => _wireframe;
            set { SetProperty(ref _wireframe, value); Properties.Settings.Default.Wireframe = value; Properties.Settings.Default.Save(); }
        }
        #endregion

        #region Commands
        public ICommand NewDocument => new RelayCommand(
            (object? obj) => { DocumentManager.NewDocument(_windowScale); },
            (object? obj) => { return true; }
        );

        public static ICommand CloseDocument => new RelayCommand(
            (object? obj) => 
            { 
                if(obj == null || obj is not Document)
                    return;

                Document? doc = obj as Document;
                if (doc == null)
                    return;

                if (doc.IsModified)
                {
                    var result = MessageBox.Show($"Do you want to save changes for {doc.Name}?", "FikusIn", MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Yes)
                        _SaveDocument(doc);
                    else if (result == MessageBoxResult.Cancel)
                        return;
                }

                DocumentManager.CloseDocument(doc);
            },
            (object? obj) => { return obj != null && obj is Document; }
        );

        public static ICommand SaveDocument => new RelayCommand(
            (object? obj) => { _SaveDocument(DocumentManager.GetActiveDocument()); },
            (object? obj) => { return DocumentManager.GetActiveDocument() != null; }
        );

        public static ICommand SaveDocumentAs => new RelayCommand(
            (object? obj) => { _SaveDocumentAs(DocumentManager.GetActiveDocument()); },
            (object? obj) => { return DocumentManager.GetActiveDocument() != null; }
        );

        public static ICommand SaveAllDocuments => new RelayCommand(
            (object? obj) => { _SaveAllDocuments(); },
            (object? obj) => { return DocumentManager.GetDocuments().Count > 0; }
        );

        public ICommand OpenDocument => new RelayCommand(
            (object? obj) => 
            {
                var dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "Supported Files (*.fikus, *.dwg, *.dxf, *.stp, *.step, *.igs, *.iges, *.brep, *.obj)|*.fikus;*.dwg;*.dxf;*.stp;*.step;*.igs;*.iges;*.brep;*.obj";
                dlg.Multiselect = true;
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.Title = "Open or Import Document";
                dlg.ValidateNames = true;

                if (dlg.ShowDialog() == true)
                    foreach (var path in dlg.FileNames)
                        DocumentManager.OpenDocument(path, _windowScale);
            },
            (object? obj) => { return true; }
        );


        public static ICommand SetActiveDocument => new RelayCommand(
            (object? obj) => { DocumentManager.SetActiveDocument(obj as Document); },
            (object? obj) => { return obj != null; }
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

        private static bool? _SaveDocumentAs(Document? p_Doc)
        {
            if (p_Doc == null)
                return false;

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "FikusIn Document (*.fikus)|*.fikus",
                FileName = p_Doc.Name,
                DefaultExt = ".fikus",
                AddExtension = true,
                OverwritePrompt = true,
                CheckPathExists = true,
                CreateTestFile = true,
                DereferenceLinks = true,
                ValidateNames = true,
                Title = "Save FikusIn Document As"
            };

            var res = dlg.ShowDialog();
            if (res == true)
                return p_Doc.SaveAs(dlg.FileName);
            else if (res == null)
                return null;
            else
                return false;
        }

        private static bool? _SaveDocument(Document? p_Doc)
        {
            if (p_Doc == null || !p_Doc.IsModified)
                return false;

            if (p_Doc.Path == "" || !p_Doc.Path.ToLower().EndsWith(Document.FikusExtension))
                return _SaveDocumentAs(p_Doc);
            else
                return p_Doc.Save();
        }

        private static void _SaveAllDocuments()
        {
            foreach (var doc in DocumentManager.GetDocuments())
                if(_SaveDocument(doc) == null)
                    break;
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
