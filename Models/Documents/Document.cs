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
    public class Document(Guid _id, string _name, bool p_isActive): ObservableObjectBase
    {
        public Guid Id { get; set; } = _id;
        public string Name { get; set; } = _name;


        private bool _isActive = p_isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        // TODO: SetProperty setup
        public string Title
        {
            get { return Name + (IsModified ? "*" : ""); }
        }

        public bool IsModified { get; private set; } = false;

        internal bool Close()
        {
            return true;
        }
    }
}
