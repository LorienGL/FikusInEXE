using FikusIn.Commands;
using FikusIn.Model.Documents;
using FikusIn.Models;
using FikusIn.Models.Documents;
using FikusIn.Utils;
using FikusIn.ViewModels;
using FikusIn.Views;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public ObservableCollection<DocumentInfo> RecentDocuments { get; private set; }

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
                    if (doc != null)
                        doc.WindowScaleInverted = 1.0 / value * _graphicsQuality;
                Properties.Settings.Default.WindowScale = value;
                Properties.Settings.Default.Save();
            }
        }

        private double _graphicsQuality = Properties.Settings.Default.GraphicsQuality;
        public double GraphicsQuality
        {
            get => _graphicsQuality;
            set
            {
                SetProperty(ref _graphicsQuality, value);
                foreach (var doc in Documents)
                    if (doc != null)
                        doc.WindowScaleInverted = 1.0 / _windowScale * value;
                Properties.Settings.Default.GraphicsQuality = value;
                Properties.Settings.Default.Save();
            }
        }

        private bool _wireframe = Properties.Settings.Default.Wireframe;
        public bool Wireframe
        {
            get => _wireframe;
            set { SetProperty(ref _wireframe, value); Properties.Settings.Default.Wireframe = value; Properties.Settings.Default.Save(); }
        }

        public int RecentDocumentsColumns
        {
            get
            {
                if (RecentDocuments.Count <= 4)
                    return RecentDocuments.Count;

                return 4;
            }
        }


        #endregion

        #region Commands
        public ICommand NewDocument => new RelayCommand(
            (object? obj) => { DocumentManager.NewDocument(_windowScale, _graphicsQuality); },
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
                        DocumentManager.OpenDocument(path, _windowScale, _graphicsQuality);
            },
            (object? obj) => { return true; }
        );

        public ICommand OpenRecentDocument => new RelayCommand(
            (object? obj) =>
            {
                if(obj == null || obj is not DocumentInfo)
                    return;

                DocumentManager.OpenDocument((obj as DocumentInfo).Path, _windowScale, _graphicsQuality);
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

        public ICommand SetLowestGraphicsQuality => new RelayCommand(
            (object? obj) => { GraphicsQuality = 2.0; },
            (object? obj) => { return true; }
        );

        public ICommand SetLowGraphicsQuality => new RelayCommand(
            (object? obj) => { GraphicsQuality = 1.66; },
            (object? obj) => { return true; }
        );

        public ICommand SetMidGraphicsQuality => new RelayCommand(
            (object? obj) => { GraphicsQuality = 1.33; },
            (object? obj) => { return true; }
        );

        public ICommand SetHighGraphicsQuality => new RelayCommand(
            (object? obj) => { GraphicsQuality = 1.0; },
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

            RecentDocuments = DocumentManager.GetRecentDocuments();
            RecentDocuments.CollectionChanged += RecentDocuments_CollectionChanged;

            var docNames = Documents.Select(d => d.Name).ToList();
            docNames.Sort(StringComparer.CurrentCultureIgnoreCase);

            ProgressList = Progress.StartProgressList();
        }

        private void RecentDocuments_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("RecentDocumentsColumns");
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
            {
                var r = p_Doc.SaveAs(dlg.FileName);
                if(r)
                    Models.Documents.RecentDocuments.Add(p_Doc, DocumentGFX.GetScreenshot(DocumentWindow.DocumentsGrid[p_Doc]));
                return r;
            }
            else if (res == null)
                return null;
            else
                return false;
        }

        private static bool? _SaveDocument(Document? p_Doc)
        {
            if (p_Doc == null)
                return false;

            if (p_Doc.Path == "" || !p_Doc.Path.ToLower().EndsWith(Document.FikusExtension))
                return _SaveDocumentAs(p_Doc);
            else
            {
                var r = p_Doc.Save();
                if(r)
                    Models.Documents.RecentDocuments.Add(p_Doc, DocumentGFX.GetScreenshot(DocumentWindow.DocumentsGrid[p_Doc]));
                return r;
            }
        }

        private static void _SaveAllDocuments()
        {
            foreach (var doc in DocumentManager.GetDocuments())
                if(_SaveDocument(doc) == null)
                    break;
        }

        private void Documents_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(DocumentManager.GetDocuments().Count == 0)
                Application.Current.Shutdown();
        }
    }
}
