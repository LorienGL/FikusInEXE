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
    public class Document: ObservableObjectBase
    {
        public Document(Guid _id, string p_name, string p_path = "", bool p_isActive = true, double p_windowScale = 1, double p_grapgicsQuality = 1)
        {
            Id = _id;
            _name = p_name;
            Path = p_path;
            _isActive = p_isActive;
            _windowScaleInverted = 1 / p_windowScale * p_grapgicsQuality;

            m_OCDoc = Load();
        }

        public Guid Id { get; private set; }

        private string _path = "";
        public string Path 
        { get => _path;
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
            if (Path == "" || !Path.ToLower().EndsWith(FikusExtension))
                return false;

            if (!m_OCDoc.Save())
                return false;

            IsModified = false;
            return true;
        }

        public bool SaveAs(String p_FileName)
        {
            if(!m_OCDoc.SaveAs(p_FileName))
                return false;

            IsModified = false;
            Path = p_FileName;
            Name = System.IO.Path.GetFileNameWithoutExtension(p_FileName);

            return true;
        }

        public override string ToString() => Name;

        private readonly OCDocument m_OCDoc;

        public OCDocument GetOCDocument() => m_OCDoc;

        private void OnNewMessage(OCMessageType p_Type, String p_Msg)
        {

        }

        public DocumentGFX? GFX { get; private set; }

        public void InitGFX()
        {
            if(GFX != null)
                return;

            GFX = new DocumentGFX(this);
        }

        public static readonly string FikusExtension = ".fikus";

        private OCDocument Load()
        {
            IsModified = false;
            
            if (Path == "")
                return OCDocument.Create(new OCMessageDelegate(this.OnNewMessage));
            else if (System.IO.Path.GetExtension(Path).ToLower() != FikusExtension)
            {
                IsModified = true; // If the file is not a Fikus file nor a new file, it is modified
                Name = System.IO.Path.GetFileNameWithoutExtension(Path);
                return OCDocument.Open(Path, new OCMessageDelegate(this.OnNewMessage));
            }
            else 
            {
                // Create a document tmp folder
                var l_TmpFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FikusIn", Guid.NewGuid().ToString());

                // Unzip the file into the temp folder
                ZipFile.ExtractToDirectory(Path, l_TmpFolder, true);

                // Load the OC document
                var l_OCDoc = OCDocument.Open(System.IO.Path.Combine(l_TmpFolder, "document.ocd"), new OCMessageDelegate(this.OnNewMessage));

                // Load the CAM document

                Name = System.IO.Path.GetFileNameWithoutExtension(Path);

                return l_OCDoc;
            }
        }
    }
}
