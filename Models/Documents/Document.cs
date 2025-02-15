using FikusIn.Models.Documents;
using FikusIn.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Document(Guid _id, string p_name, string p_path = "", bool p_isActive = true)
        {
            Id = _id;
            _name = p_name;
            Path = p_path;
            _isActive = p_isActive;

            if(Path == "")
                m_OCDoc = OCDocument.Create(new OCMessageDelegate(this.OnNewMessage));
            else
                m_OCDoc = OCDocument.Open(Path, new OCMessageDelegate(this.OnNewMessage));
        }

        public Guid Id { get; private set; }

        public string Path { get; private set; }

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

        internal bool Close()
        {
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
    }
}
