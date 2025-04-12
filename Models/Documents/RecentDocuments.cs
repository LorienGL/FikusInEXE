using FikusIn.Model.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FikusIn.Models.Documents
{
    public class DocumentInfo
    {
        public string Path { get; set; }
        public BitmapEncoder Icon { get; set; }

        public DocumentInfo(string path, BitmapEncoder icon)
        {
            Path = path;
            Icon = icon;
        }
    }

    public class RecentDocuments
    {
        private static ObservableCollection<DocumentInfo> _recentDocuments = [];

        public static void Add(Document document, BitmapEncoder? icon)
        {
            if (document == null || document.Path == "" || icon == null)
                return;

            foreach (var di in _recentDocuments)
            {
                if (di.Path != document.Path)
                    continue;

                _recentDocuments.Remove(di);
                break;
            }

            _recentDocuments.Insert(0, new DocumentInfo(document.Path, icon));
            if (_recentDocuments.Count > 20)
                _recentDocuments.RemoveAt(20);

            SaveRecentDocuments();
        }

        private static bool _documentsLoaded = false;

        public static ObservableCollection<DocumentInfo> GetRecentDocuments()
        {
            if (!_documentsLoaded)
                LoadRecentDocuments();
            return _recentDocuments;
        }

        public static void LoadRecentDocuments()
        {
            // Load recent documents from settings

            _documentsLoaded = true;

        }

        private static void SaveRecentDocuments()
        {
            
            // Save recent documents to settings
            foreach (var di in _recentDocuments)
            {
                // Save di.Path
                //if (File.Exists(fileName))
                //    File.Delete(fileName);
                //using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                //{
                //    pngBitmapEncoder.Save(fileStream);
                //}

            }
        }
    }
}
