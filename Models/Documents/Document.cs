using FikusIn.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FikusIn.Model.Documents
{
    /// <summary>
    /// Document class 
    /// Do not create documents using new Document(...) unless for testing purposes, use DocumentManager.NewDocument(...) instead
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="_name"></param>
    /// <param name="p_isActive"></param>
    public class Document(Guid _id, string p_name, bool p_isActive): ObservableObjectBase
    {
        public Guid Id { get; set; } = _id;

        private string _name = p_name;
        public string Name 
        { 
            get => _name; 
            set => SetProperty(ref _name, value);
        }

        private bool _isActive = p_isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private bool _isModified = true;
        public bool IsModified 
        {
            get => _isModified;
            private set => SetProperty(ref _isModified, value); 
        }

        internal bool Close()
        {
            return true;
        }

        public void Save()
        {
            IsModified = false;
        }

        public override string ToString() => Name;
    }
}
