using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FikusIn.Model.Documents
{
    public class Document(Guid _id, string _name, bool _isActive)
    {
        public Guid Id { get; set; } = _id;
        public string Name { get; set; } = _name;
        public bool IsActive { get; set; } = _isActive;
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
