using FikusIn.Model.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FikusIn.Models.Documents
{
    public class DocumentInfo
    {
        public string Path { get; set; }
        public Image Thumbnail { get; set; }

        public DocumentInfo(string path, Image icon)
        {
            Path = path;
            Thumbnail = icon;
        }
    }

    public class RecentDocuments
    {
        private static ObservableCollection<DocumentInfo> _recentDocuments = [];

        public static void Add(Document? document, Image? icon)
        {
            if (document == null || document.Path == "" || icon == null)
                return;

            Load();

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

            Save();
        }

        public static ObservableCollection<DocumentInfo> Get()
        {
            Load();
            return _recentDocuments;
        }

        private static readonly string configFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FikusIn", "RecentDocuments.txt");
        private static readonly string imagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FikusIn", "RecentDocumentsImages");

        private static void Load()
        {
            if (_recentDocuments.Count > 0)
                return;

            // Load recent documents from settings
            if (!File.Exists(configFileName))
                return;
            if (!Directory.Exists(imagesPath))
                return;

            using (StreamReader reader = new StreamReader(configFileName))
            {
                string? line;
                int i = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var imageFileName = Path.Combine(imagesPath, $"{i++}.png");
                    if (!File.Exists(imageFileName))
                        continue;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new System.Uri(imageFileName);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Load the full image immediately
                    bitmapImage.EndInit();

                    if (bitmapImage.PixelWidth == 0 || bitmapImage.PixelHeight == 0)
                        continue;

                    Image image = new();
                    image.Source = bitmapImage;                  

                    _recentDocuments.Add(new DocumentInfo(line, image));
                }
            }
        }

        private static void Save()
        {
            if (File.Exists(configFileName))
                File.Delete(configFileName);
            
            if (_recentDocuments.Count == 0)
                return;

            // Create FinusIn directory if it doesn't exist
            string? directoryPath = Path.GetDirectoryName(configFileName);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            using (StreamWriter writer = new StreamWriter(configFileName))
            {
                int i = 0;
                // Save recent documents to settings
                foreach (var di in _recentDocuments)
                {
                    // Save di.Path
                    writer.WriteLine(di.Path);

                    var imageFileName = Path.Combine(imagesPath, $"{i++}.png");

                    if (!Directory.Exists(imagesPath))
                        Directory.CreateDirectory(imagesPath);

                    if (File.Exists(imageFileName))
                        File.Delete(imageFileName);

                    using (FileStream fileStream = new FileStream(imageFileName, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(di.Thumbnail.Source as BitmapSource));
                        encoder.Save(fileStream);
                    }
                }
            }
        }
    }
}
