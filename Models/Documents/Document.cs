using FikusIn.Models;
using FikusIn.Models.Documents;
using FikusIn.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;
using static FikusIn.Models.Message;

namespace FikusIn.Model.Documents
{
    /// <summary>
    /// Document class 
    /// Do not create documents using new Document(...) unless for testing purposes, use DocumentManager.NewDocument(...) instead
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="_name"></param>
    /// <param name="p_isActive"></param>
    public class Document : ObservableObjectBase
    {
        private readonly OCDocument? m_OCDoc;

        public Document(Guid _id, string p_name, string p_path = "", bool p_isActive = true, double p_windowScale = 1, double p_grapgicsQuality = 1)
        {
            Id = _id;
            _name = p_name;
            Path = p_path;
            _isActive = p_isActive;
            _windowScaleInverted = 1 / p_windowScale * p_grapgicsQuality;

            m_OCDoc = _Load();
        }

        public Guid Id { get; private set; }

        private string _path = "";
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private bool _isModified = false;
        public bool IsModified
        {
            get => _isModified;
            private set => SetProperty(ref _isModified, value);
        }

        private double _windowScaleInverted = 1;
        public double WindowScaleInverted
        {
            get => _windowScaleInverted;
            set => SetProperty(ref _windowScaleInverted, value);
        }

        internal bool Close()
        {
            GFX?.Dispose();
            return true;
        }

        public bool Save()
        {
            if (Path == "")
                return false;

            _Save();

            return true;
        }

        public bool SaveAs(String p_FileName)
        {
            Path = p_FileName;
            Name = System.IO.Path.GetFileNameWithoutExtension(p_FileName);

            _Save();

            return true;
        }

        public override string ToString() => Name;

        public OCDocument? GetOCDocument() => m_OCDoc;

        private void OnNewMessage(OCMessageType p_Type, String p_Msg)
        {

        }

        public DocumentGFX? GFX { get; private set; }

        public void InitGFX()
        {
            if (GFX != null)
                return;

            GFX = new DocumentGFX(this);
        }

        public static readonly string FikusExtension = ".fikus";

        private string m_TmpFolder = "";
        private OCDocument? _Load()
        {
            IsModified = false;

            // Create a document tmp folder
            m_TmpFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FikusIn", Id.ToString());
            System.IO.Directory.CreateDirectory(m_TmpFolder);

            if (Path == "")
                return OCDocument.Create(new OCMessageDelegate(this.OnNewMessage));
            else
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(Path);

                if (System.IO.Path.GetExtension(Path).ToLower() != FikusExtension)
                {
                    IsModified = true; // If the file is not a Fikus file nor a new file, it is modified
                    var l_OCDoc = OCDocument.Open(Path, new OCMessageDelegate(this.OnNewMessage));

                    if (l_OCDoc != null)
                        l_OCDoc.SaveAs(System.IO.Path.Combine(m_TmpFolder, "document.ocd.xbf"));

                    return l_OCDoc;
                }
                else
                {
                    // Unzip the file into the temp folder
                    ZipFile.ExtractToDirectory(Path, m_TmpFolder, true);

                    // Load the OC document
                    var l_OCDoc = OCDocument.Open(System.IO.Path.Combine(m_TmpFolder, "document.ocd.xbf"), new OCMessageDelegate(this.OnNewMessage));

                    // Load the CAM document

                    Name = System.IO.Path.GetFileNameWithoutExtension(Path);

                    return l_OCDoc;
                }
            }
        }

        public bool _Save()
        {
            if (Path == "")
                return false;

            try
            {
                if (System.IO.Path.GetExtension(Path).ToLower() != FikusExtension)
                    Path += FikusExtension;

                // Save CAD file
                if (m_OCDoc != null)
                    m_OCDoc.SaveAs(System.IO.Path.Combine(m_TmpFolder, "document.ocd"));

                // Save CAM file

                // Create a zip file with all files in the temp folder
                var mode = ZipArchiveMode.Create;
                if (System.IO.File.Exists(Path))
                    mode = ZipArchiveMode.Update;
                using (var l_Zip = ZipFile.Open(Path, mode))
                {
                    foreach (var l_File in System.IO.Directory.GetFiles(m_TmpFolder))
                        l_Zip.CreateEntryFromFile(l_File, System.IO.Path.GetFileName(l_File));
                }

                IsModified = false;
                return true;
            }
            catch (Exception e)
            {              
                Messenger.ShowError("Error saving file: " + e.ToString());
                return false;
            }
        }

        public void Dispose()
        {
            GFX?.Dispose();
            GFX = null;
            if (m_OCDoc != null)
            {
                m_OCDoc?.Dispose();
            }

            if (m_TmpFolder == "")
                return;
            try
            {
                System.IO.Directory.Delete(m_TmpFolder, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
