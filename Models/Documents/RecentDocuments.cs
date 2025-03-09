using FikusIn.Model.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FikusIn.Models.Documents
{
    public class DocumentInfo
    {
        public string Path { get; set; }
        public BitmapImage Icon { get; set; }
    }

    public class RecentDocuments
    {
        private static ObservableCollection<DocumentInfo> _recentDocuments = [];

        public static void Add(Document document, BitmapImage icon)
        {
            if (document == null || document.Path == "")
                return;

            foreach (var di in _recentDocuments)
            {
                if (di.Path != document.Path)
                    continue;

                _recentDocuments.Remove(di);
                break;
            }

            _recentDocuments.Insert(0, new DocumentInfo() { Path = document.Path, Icon = icon });
            if (_recentDocuments.Count > 20)
                _recentDocuments.RemoveAt(20);
        }

        public static ObservableCollection<DocumentInfo> GetRecentDocuments()
        {
            return _recentDocuments;
        }

        public static void LoadRecentDocuments()
        {
            // Load recent documents from settings

        }

        private static void SaveRecentDocuments()
        {
            
            // Save recent documents to settings
            foreach (var di in _recentDocuments)
            {
                // Save di.Path
                
            }
        }
    }
}
